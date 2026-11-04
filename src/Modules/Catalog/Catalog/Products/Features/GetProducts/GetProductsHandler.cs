using BuildingBlocks.Common.Pagination;
using Catalog.Contracts.Features;

namespace Catalog.Products.Features.GetProducts
{
    public record GetProductsQuery(PaginationRequest PaginationRequest) : IQuery<GetProductsResult>;

    public record GetProductsResult(PaginatedResult<ProductDto> Products);

    internal sealed class GetProductsQueryHandler(ICatalogDbContext catalogDbContext) : IQueryHandler<GetProductsQuery, GetProductsResult>
    {
        private const int MaxPageSize = 100;

        public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            var pageNumber = query.PaginationRequest.PageNumber < 0 ? 0 : query.PaginationRequest.PageNumber;
            var pageSize = query.PaginationRequest.PageSize < 1 ? 10 : Math.Min(query.PaginationRequest.PageSize, MaxPageSize);

            var totalCount = await catalogDbContext.Products.LongCountAsync(cancellationToken);
            var products = await catalogDbContext.Products.AsNoTracking()
                                                          .OrderByDescending(p => p.CreatedAt)
                                                          .Skip(pageNumber * pageSize)
                                                          .Take(pageSize)
                                                          .ToListAsync(cancellationToken);

            return new GetProductsResult(new PaginatedResult<ProductDto>(pageNumber, pageSize, totalCount, products.ToDtoList()));
        }
    }

    public record GetProductsResponse(PaginatedResult<ProductDto> Products);

    public class GetProductsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products", async ([AsParameters] PaginationRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetProductsQuery(request), cancellationToken);

                var response = new GetProductsResponse(result.Products);

                return Results.Ok(response);
            })
            .WithName("GetProducts")
            .WithTags("Products")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK);
        }
    }
}