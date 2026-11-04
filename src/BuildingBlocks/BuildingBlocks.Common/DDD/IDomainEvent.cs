using MediatR;

namespace BuildingBlocks.Common.DDD
{
    public interface IDomainEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }

    public record DomainEvent : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => GetType().AssemblyQualifiedName!;
    }
}