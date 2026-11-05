using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace BuildingBlocks.Messaging.Outbox
{
    public sealed class OutboxProcessor<TContext>(IServiceScopeFactory scopeFactory,
                                                  IOptions<OutboxOptions> options,
                                                  ILogger<OutboxProcessor<TContext>> logger) : BackgroundService
        where TContext : DbContext
    {
        private static readonly Assembly EventsAssembly = typeof(IntegrationEvent).Assembly;
        private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();

        private readonly OutboxOptions _options = options.Value;
        private string? _lockQuery;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                logger.LogInformation("Outbox processor ({Context}) is disabled.", typeof(TContext).Name);
                return;
            }

            logger.LogInformation("Outbox processor ({Context}) started. BatchSize={BatchSize}, Interval={Interval}s",
                typeof(TContext).Name, _options.BatchSize, _options.PollingIntervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                var hasMore = false;

                try
                {
                    hasMore = await ProcessBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Outbox processor ({Context}) failed while processing a batch.", typeof(TContext).Name);
                }

                // Batch doluysa bekleme yapmadan kuyruğu boşaltmaya devam et.
                if (hasMore) continue;

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            logger.LogInformation("Outbox processor ({Context}) stopped.", typeof(TContext).Name);
        }

        private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            // FOR UPDATE SKIP LOCKED: birden fazla instance çalışsa bile aynı mesaj iki kez alınmaz.
            var messages = await dbContext.Set<OutboxMessage>()
                                          .FromSqlRaw(GetLockQuery(dbContext), _options.MaxRetryCount, _options.BatchSize)
                                          .ToListAsync(cancellationToken);

            if (messages.Count == 0)
                return false;

            var publishFailed = false;

            foreach (var message in messages)
            {
                try
                {
                    var messageType = ResolveType(message.Type);
                    if (messageType is null)
                    {
                        MarkAsPoison(message, $"Unknown message type '{message.Type}'.");
                        continue;
                    }

                    var payload = JsonSerializer.Deserialize(message.Content, messageType);
                    if (payload is null)
                    {
                        MarkAsPoison(message, "Message content is empty.");
                        continue;
                    }

                    await publishEndpoint.Publish(payload, messageType, cancellationToken);

                    message.ProcessedOn = DateTime.UtcNow;
                    message.Error = null;
                }
                catch (JsonException ex)
                {
                    // Deserialize edilemeyen mesaj tekrar denense de düzelmez.
                    MarkAsPoison(message, ex.Message);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    message.RetryCount++;
                    message.Error = Truncate(ex.ToString());

                    logger.LogWarning(ex, "Failed to publish outbox message {MessageId} ({Type}). RetryCount={RetryCount}",
                        message.Id, message.Type, message.RetryCount);

                    // Sıralamayı korumak ve broker kapalıyken diğer mesajların retry'ını tüketmemek için turu bitir.
                    publishFailed = true;
                    break;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return !publishFailed && messages.Count == _options.BatchSize;
        }

        private void MarkAsPoison(OutboxMessage message, string error)
        {
            message.RetryCount = _options.MaxRetryCount;
            message.Error = Truncate(error);

            logger.LogError("Outbox message {MessageId} ({Type}) is marked as poison: {Error}",
                message.Id, message.Type, error);
        }

        private static Type? ResolveType(string typeName) => TypeCache.GetOrAdd(typeName, name => EventsAssembly.GetType(name));

        private static string Truncate(string value, int maxLength = 4000) => value.Length <= maxLength ? value : value[..maxLength];

        // Tablo/şema/kolon adları EF metadata'sından alınır (snake_case convention ile uyumlu kalır).
        private string GetLockQuery(DbContext dbContext)
        {
            if (_lockQuery is not null)
                return _lockQuery;

            var entityType = dbContext.Model.FindEntityType(typeof(OutboxMessage))
                ?? throw new InvalidOperationException($"{typeof(TContext).Name} does not contain {nameof(OutboxMessage)}.");

            var tableName = entityType.GetTableName()!;
            var schema = entityType.GetSchema();
            var table = StoreObjectIdentifier.Table(tableName, schema);

            string Column(string property) => $"\"{entityType.FindProperty(property)!.GetColumnName(table)}\"";

            var qualifiedTable = schema is null ? $"\"{tableName}\"" : $"\"{schema}\".\"{tableName}\"";
            var processedOn = Column(nameof(OutboxMessage.ProcessedOn));
            var retryCount = Column(nameof(OutboxMessage.RetryCount));
            var occurredOn = Column(nameof(OutboxMessage.OccurredOn));

            return _lockQuery = $$"""
                                  SELECT * FROM {{qualifiedTable}}
                                  WHERE {{processedOn}} IS NULL AND {{retryCount}} < {0}
                                  ORDER BY {{occurredOn}}
                                  LIMIT {1}
                                  FOR UPDATE SKIP LOCKED
                                  """;
        }
    }
}
