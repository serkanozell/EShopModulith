using Basket.Baskets.Features.UpdateItemPriceInBasket;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Basket.Baskets.EventHandlers
{
    public sealed class ProductPriceChangedIntegrationEventHandler(BasketDbContext dbContext, ILogger<ProductPriceChangedIntegrationEventHandler> logger)
        : InboxConsumer<BasketDbContext, ProductPriceChangedIntegrationEvent>(dbContext, logger);

    internal sealed class ProductPriceChangedInboxHandler(ISender sender, ILogger<ProductPriceChangedInboxHandler> logger) : InboxMessageHandler<ProductPriceChangedIntegrationEvent>
    {
        public override async Task Handle(ProductPriceChangedIntegrationEvent message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing ProductPriceChangedIntegrationEvent: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
                message.ProductId, message.OldPrice, message.NewPrice);

            var command = new UpdateItemPriceInBasketCommand(message.ProductId, message.NewPrice, message.OccurredOn);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Successfully updated item price in basket for ProductId: {ProductId}", message.ProductId);
            }
            else
            {
                logger.LogError("Failed to update item price in basket for ProductId: {ProductId}.", message.ProductId);
            }
        }
    }
}
//public sealed class ProductPriceChangedIntegrationEventHandler(ISender sender, ILogger<ProductPriceChangedIntegrationEventHandler> logger) : IConsumer<ProductPriceChangedIntegrationEvent>
//{
//    public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
//    {
//        logger.LogInformation("Received ProductPriceChangedIntegrationEvent: {ProductId}, OldPrice: {OldPrice}, NewPrice: {NewPrice}",
//            context.Message.ProductId, context.Message.OldPrice, context.Message.NewPrice);

//        var command = new UpdateItemPriceInBasketCommand(context.Message.ProductId, context.Message.NewPrice, context.Message.OccurredOn);

//        var result = await sender.Send(command, context.CancellationToken);

//        if (result.IsSuccess)
//        {
//            logger.LogInformation("Successfully updated item price in basket for ProductId: {ProductId}", context.Message.ProductId);
//        }
//        else
//        {
//            logger.LogError("Failed to update item price in basket for ProductId: {ProductId}.", context.Message.ProductId);
//        }
//    }
//}
//}