using BuildingBlocks.Common.DDD;

namespace Basket.Baskets.Events
{
    public record BasketCreatedEvent(Guid BasketId, string UserName) : DomainEvent;

}