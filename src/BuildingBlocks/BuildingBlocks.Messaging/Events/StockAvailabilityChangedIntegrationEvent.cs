namespace BuildingBlocks.Messaging.Events
{
    public record StockAvailabilityChangedIntegrationEvent : IntegrationEvent
    {
        public Guid InventoryItemId { get; set; } = default!;
        public Guid ProductId { get; set; } = default!;
        public bool IsInStock { get; set; }
        public int AvailableQuantity { get; set; } = default!;
    }
}