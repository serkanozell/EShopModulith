using BuildingBlocks.Common.DDD;

namespace Basket.Baskets.Events
{
    public record BasketItemRemovedEvent(Guid BasketId, Guid ProductId) : DomainEvent;

}