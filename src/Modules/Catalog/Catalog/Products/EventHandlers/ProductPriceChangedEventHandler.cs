using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Outbox;
using Catalog.Products.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Catalog.Products.EventHandlers
{
    public sealed class ProductPriceChangedEventHandler(ICatalogDbContext catalogDbContext, ILogger<ProductPriceChangedEventHandler> logger) : INotificationHandler<ProductPriceChangedEvent>
    {
        public Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Domain event handled: {DomainEvent} - ProductId: {ProductId}, {OldPrice} -> {NewPrice}",
                                  notification.GetType().Name,
                                  notification.ProductId.Value,
                                  notification.OldPrice,
                                  notification.NewPrice);


            var productPriceChangedIntegrationEvent = new ProductPriceChangedIntegrationEvent
            {
                ProductId = notification.ProductId.Value,
                OldPrice = notification.OldPrice,
                NewPrice = notification.NewPrice
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),

                Type = typeof(ProductPriceChangedIntegrationEvent).FullName!,

                Content = JsonSerializer.Serialize(productPriceChangedIntegrationEvent),

                OccurredOn = DateTime.UtcNow
            };

            catalogDbContext.OutboxMessages.Add(outboxMessage);

            return Task.CompletedTask;
        }
    }
}