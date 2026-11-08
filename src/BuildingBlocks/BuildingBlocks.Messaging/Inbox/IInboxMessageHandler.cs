namespace BuildingBlocks.Messaging.Inbox
{
    public interface IInboxMessageHandler
    {
        Task Handle(object message, CancellationToken cancellationToken);
    }
}