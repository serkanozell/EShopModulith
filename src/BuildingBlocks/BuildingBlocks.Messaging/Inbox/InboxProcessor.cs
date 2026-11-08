using BuildingBlocks.Messaging.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace BuildingBlocks.Messaging.Inbox
{
    public sealed class InboxProcessor<TContext>(IServiceScopeFactory scopeFactory,
                                                     IOptions<InboxOptions> options,
                                                     InboxHandlerRegistry<TContext> registry,
                                                     ILogger<InboxProcessor<TContext>> logger) : BackgroundService
            where TContext : DbContext
    {
        private static readonly Assembly EventsAssembly = typeof(IntegrationEvent).Assembly;
        private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

        private readonly InboxOptions _options = options.Value;
        private string? _lockQuery;
        private DateTime _lastCleanupUtc = DateTime.MinValue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                logger.LogInformation("Inbox processor ({Context}) is disabled.", typeof(TContext).Name);
                return;
            }

            logger.LogInformation("Inbox processor ({Context}) started. Interval={Interval}s",
                typeof(TContext).Name, _options.PollingIntervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                var processed = false;

                try
                {
                    await CleanupIfDueAsync(stoppingToken);
                    processed = await ProcessNextAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Inbox processor ({Context}) failed.", typeof(TContext).Name);
                }

                // Mesaj işlendiyse beklemeden kuyruğu boşaltmaya devam et.
                if (processed) continue;

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            logger.LogInformation("Inbox processor ({Context}) stopped.", typeof(TContext).Name);
        }

        /// <returns>true: bir mesaj tamamlandı (işlendi veya poison). false: mesaj yok ya da işleme hata verdi.</returns>
        private async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
        {
            Guid failedMessageId;
            Exception failure;

            await using (var scope = scopeFactory.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

                // Handler'ın SaveChanges'i de bu transaction'a katılır: iş etkisi + processed_on birlikte commit olur.
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                // FOR UPDATE SKIP LOCKED: birden fazla instance çalışsa bile aynı mesaj iki kez alınmaz.
                var message = (await dbContext.Set<InboxMessage>()
                                              .FromSqlRaw(GetLockQuery(dbContext), _options.MaxRetryCount)
                                              .ToListAsync(cancellationToken))
                              .FirstOrDefault();

                if (message is null)
                    return false;

                if (!TryDeserialize(message, out var messageType, out var payload, out var parseError))
                {
                    MarkAsPoison(message, parseError);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }

                var handlerTypes = registry.GetHandlerTypes(messageType);
                if (handlerTypes.Count == 0)
                {
                    MarkAsPoison(message, $"No inbox handler registered for '{message.Type}'.");
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }

                try
                {
                    foreach (var handlerType in handlerTypes)
                    {
                        var handler = (IInboxMessageHandler)scope.ServiceProvider.GetRequiredService(handlerType);
                        await handler.Handle(payload, cancellationToken);
                    }

                    message.ProcessedOn = DateTime.UtcNow;
                    message.Error = null;

                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    failedMessageId = message.Id;
                    failure = ex;
                    // Scope'tan çıkınca transaction commit edilmeden dispose olur => rollback.
                }
            }

            await RegisterFailureAsync(failedMessageId, failure);
            return false;
        }

        // Rollback sonrası temiz bir context ile retry sayacını artırır.
        private async Task RegisterFailureAsync(Guid messageId, Exception exception)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

            var error = Truncate(exception.ToString());

            await dbContext.Set<InboxMessage>()
                           .Where(x => x.Id == messageId)
                           .ExecuteUpdateAsync(s => s.SetProperty(x => x.RetryCount, x => x.RetryCount + 1)
                                                     .SetProperty(x => x.Error, error),
                                               CancellationToken.None);

            logger.LogWarning(exception, "Failed to process inbox message {MessageId}. It will be retried until MaxRetryCount.", messageId);
        }

        private bool TryDeserialize(InboxMessage message, out Type messageType, out object payload, out string error)
        {
            messageType = default!;
            payload = default!;
            error = string.Empty;

            var type = ResolveType(message.Type);
            if (type is null)
            {
                error = $"Unknown message type '{message.Type}'.";
                return false;
            }

            try
            {
                var value = JsonSerializer.Deserialize(message.Content, type);
                if (value is null)
                {
                    error = "Message content is empty.";
                    return false;
                }

                messageType = type;
                payload = value;
                return true;
            }
            catch (JsonException ex)
            {
                // Deserialize edilemeyen mesaj tekrar denense de düzelmez.
                error = ex.Message;
                return false;
            }
        }

        private void MarkAsPoison(InboxMessage message, string error)
        {
            message.RetryCount = _options.MaxRetryCount;
            message.Error = Truncate(error);

            logger.LogError("Inbox message {MessageId} ({Type}) is marked as poison: {Error}",
                message.Id, message.Type, error);
        }

        private async Task CleanupIfDueAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            if (now - _lastCleanupUtc < CleanupInterval) return;
            _lastCleanupUtc = now;

            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

            var threshold = now.AddDays(-_options.RetentionDays);

            var deleted = await dbContext.Set<InboxMessage>()
                                         .Where(x => x.ProcessedOn != null && x.ProcessedOn < threshold)
                                         .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
                logger.LogInformation("Inbox cleanup ({Context}) removed {Count} processed messages.", typeof(TContext).Name, deleted);
        }

        private static Type? ResolveType(string typeName) => TypeCache.GetOrAdd(typeName, name => EventsAssembly.GetType(name));

        private static string Truncate(string value, int maxLength = 4000) => value.Length <= maxLength ? value : value[..maxLength];

        // Tablo/şema/kolon adları EF metadata'sından alınır (snake_case convention ile uyumlu kalır).
        private string GetLockQuery(DbContext dbContext)
        {
            if (_lockQuery is not null)
                return _lockQuery;

            var entityType = dbContext.Model.FindEntityType(typeof(InboxMessage))
                ?? throw new InvalidOperationException($"{typeof(TContext).Name} does not contain {nameof(InboxMessage)}.");

            var tableName = entityType.GetTableName()!;
            var schema = entityType.GetSchema();
            var table = StoreObjectIdentifier.Table(tableName, schema);

            string Column(string property) => $"\"{entityType.FindProperty(property)!.GetColumnName(table)}\"";

            var qualifiedTable = schema is null ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
            var processedOn = Column(nameof(InboxMessage.ProcessedOn));
            var retryCount = Column(nameof(InboxMessage.RetryCount));
            var occurredOn = Column(nameof(InboxMessage.OccurredOn));

            return _lockQuery = $$"""
                                  SELECT * FROM {{qualifiedTable}}
                                  WHERE {{processedOn}} IS NULL AND {{retryCount}} < {0}
                                  ORDER BY {{occurredOn}}
                                  LIMIT 1
                                  FOR UPDATE SKIP LOCKED
                                  """;
        }
    }
}
