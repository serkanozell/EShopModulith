namespace BuildingBlocks.Messaging.Events
{
    public record IntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string EventType => GetType().AssemblyQualifiedName!;
    }
}