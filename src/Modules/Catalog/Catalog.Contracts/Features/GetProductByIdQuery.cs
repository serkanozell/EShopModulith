using BuildingBlocks.Common.CQRS;

namespace Catalog.Contracts.Features
{
    public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

    public record GetProductByIdResult(ProductDto Product);
}
