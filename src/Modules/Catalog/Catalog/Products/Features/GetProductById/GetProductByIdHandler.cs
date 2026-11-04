using Catalog.Contracts.Features;

namespace Catalog.Products.Features.GetProductById
{
    internal sealed class GetProductByIdQueryHandler(ICatalogDbContext catalogDbContext) : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
    {
        public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            if (query.Id == Guid.Empty)
                throw new ArgumentException("Id is required.", nameof(query));

            var product = await catalogDbContext.Products.FirstOrDefaultAsync(p => p.Id == ProductId.Of(query.Id), cancellationToken)
                ?? throw new KeyNotFoundException($"Product with id '{query.Id}' was not found.");

            return new GetProductByIdResult(product.ToDto());
        }
    }
}