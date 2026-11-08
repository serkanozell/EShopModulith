using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Messaging.Inbox
{
    /// <summary>
    /// Gelen integration event'i sadece inbox tablosuna yazar (duplicate ise atlar).
    /// İş mantığı InboxProcessor tarafından IInboxMessageHandler ile çalıştırılır.
    /// </summary>
    public abstract class InboxConsumer<TContext, TMessage>(TContext dbContext, ILogger logger) : IConsumer<TMessage>
        where TContext : DbContext
        where TMessage : IntegrationEvent
    {
        public async Task Consume(ConsumeContext<TMessage> context)
        {
            var message = context.Message;
            var cancellationToken = context.CancellationToken;
            var inbox = dbContext.Set<InboxMessage>();

            if (await inbox.AnyAsync(x => x.Id == message.EventId, cancellationToken))
            {
                LogDuplicate(message);
                return;
            }

            var entry = inbox.Add(new InboxMessage
            {
                Id = message.EventId,
                Type = typeof(TMessage).FullName!,
                Content = JsonSerializer.Serialize(message),
                OccurredOn = message.OccurredOn,
                ReceivedOn = DateTime.UtcNow
            });

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Aynı anda gelen iki kopya: unique ihlali duplicate demektir.
                entry.State = EntityState.Detached;

                if (await inbox.AnyAsync(x => x.Id == message.EventId, cancellationToken))
                {
                    LogDuplicate(message);
                    return;
                }

                throw;
            }
        }

        private void LogDuplicate(TMessage message) =>
            logger.LogInformation("Duplicate {MessageType} skipped. EventId={EventId}", typeof(TMessage).Name, message.EventId);
    }
}