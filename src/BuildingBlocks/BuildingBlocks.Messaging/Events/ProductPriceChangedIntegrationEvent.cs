namespace BuildingBlocks.Messaging.Events
{
    public record ProductPriceChangedIntegrationEvent : IntegrationEvent
    {
        public Guid ProductId { get; set; } = default!;
        public decimal OldPrice { get; set; } = default!;
        public decimal NewPrice { get; set; } = default!;
    }
}