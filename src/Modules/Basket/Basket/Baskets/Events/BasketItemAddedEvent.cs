using BuildingBlocks.Common.DDD;

namespace Basket.Baskets.Events
{
    public record BasketItemAddedEvent(Guid BasketId,
                                       Guid ProductId,
                                       int Quantity,
                                       decimal Price,
                                       string ProductName) : DomainEvent;

}