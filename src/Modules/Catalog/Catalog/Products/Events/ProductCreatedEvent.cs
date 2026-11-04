using BuildingBlocks.Common.DDD;

namespace Catalog.Products.Events
{
    public record ProductCreatedEvent(ProductId ProductId,
                                      ProductName Name,
                                      List<string> Category,
                                      string? Description,
                                      decimal Price,
                                      int StockQuantity) : IDomainEvent;
}