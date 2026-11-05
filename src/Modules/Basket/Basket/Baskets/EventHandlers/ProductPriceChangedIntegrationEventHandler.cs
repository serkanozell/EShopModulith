using Basket.Baskets.Features.UpdateItemPriceInBasket;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Basket.Baskets.EventHandlers
{
    public sealed class ProductPriceChangedIntegrationEventHandler(ISender sender, ILogger<ProductPriceChangedIntegrationEventHandler> logger) : IConsumer<ProductPriceChangedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
        {
            logger.LogInformation("Received ProductPriceChangedIntegrationEvent: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                context.Message.ProductId, context.Message.OldPrice, context.Message.NewPrice);

            var command = new UpdateItemPriceInBasketCommand(context.Message.ProductId, context.Message.NewPrice, context.Message.OccurredOn);

            var result = await sender.Send(command, context.CancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Successfully updated item price in basket for ProductId: {ProductId}", context.Message.ProductId);
            }
            else
            {
                logger.LogError("Failed to update item price in basket for ProductId: {ProductId}.", context.Message.ProductId);
            }
        }
    }
}