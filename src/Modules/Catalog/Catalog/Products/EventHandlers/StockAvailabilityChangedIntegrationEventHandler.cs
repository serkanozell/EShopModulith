using BuildingBlocks.Messaging.Events;
using Catalog.Contracts.Features;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Catalog.Products.EventHandlers
{
    public sealed class StockAvailabilityChangedIntegrationEventHandler(ISender sender, ILogger<StockAvailabilityChangedIntegrationEventHandler> logger) : IConsumer<StockAvailabilityChangedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<StockAvailabilityChangedIntegrationEvent> context)
        {
            logger.LogInformation("Received StockAvailabilityChangedIntegrationEvent: {ProductId}, IsInStock: {IsInStock}",
                context.Message.ProductId, context.Message.IsInStock);

            var command = new UpdateProductStockCommand(context.Message.ProductId, context.Message.IsInStock, context.Message.AvailableQuantity, context.Message.OccurredOn);
            var result = await sender.Send(command, context.CancellationToken);
            if (result.IsSuccess)
            {
                logger.LogInformation("Successfully updated item stock availability in basket for ProductId: {ProductId}", context.Message.ProductId);
            }
            else
            {
                logger.LogError("Failed to update item stock availability in basket for ProductId: {ProductId}.", context.Message.ProductId);
            }
        }
    }
}