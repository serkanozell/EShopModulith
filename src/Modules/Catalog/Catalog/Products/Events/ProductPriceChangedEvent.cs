using BuildingBlocks.Common.DDD;

namespace Catalog.Products.Events
{
    public record ProductPriceChangedEvent(ProductId ProductId,
                                       decimal OldPrice,
                                       decimal NewPrice) : IDomainEvent;
}