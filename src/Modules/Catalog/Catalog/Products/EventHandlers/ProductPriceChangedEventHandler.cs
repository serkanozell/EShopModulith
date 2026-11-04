using BuildingBlocks.Messaging.Events;
using Catalog.Products.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Catalog.Products.EventHandlers
{
    public sealed class ProductPriceChangedEventHandler(IPublishEndpoint publishEndpoint, ILogger<ProductPriceChangedEventHandler> logger) : INotificationHandler<ProductPriceChangedEvent>
    {
        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
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

            await publishEndpoint.Publish(productPriceChangedIntegrationEvent, cancellationToken);
        }
    }
}