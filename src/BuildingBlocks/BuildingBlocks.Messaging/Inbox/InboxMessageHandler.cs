using BuildingBlocks.Messaging.Events;

namespace BuildingBlocks.Messaging.Inbox
{
    /// <summary>
    /// Inbox'tan işlenecek mesajlar için taban sınıf. Handler, InboxProcessor'ın
    /// açtığı transaction içinde çalışır; SaveChanges çağrıları aynı transaction'a katılır.
    /// </summary>
    public abstract class InboxMessageHandler<TMessage> : IInboxMessageHandler where TMessage : IntegrationEvent
    {
        public abstract Task Handle(TMessage message, CancellationToken cancellationToken);

        Task IInboxMessageHandler.Handle(object message, CancellationToken cancellationToken)
            => Handle((TMessage)message, cancellationToken);
    }
}