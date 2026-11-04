using Catalog.Contracts.Features;

namespace Catalog.Products.Features.GetProductByCategory
{
    public record GetProductByCategoryQuery(string Category) : IQuery<GetProductByCategoryResult>;
    public record GetProductByCategoryResult(IEnumerable<ProductDto> Products);

    internal sealed class GetProductByCategoryHandler(ICatalogDbContext catalogDbContext) : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
    {
        public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
        {
            // get products by category using dbContext
            // return result

            var products = await catalogDbContext.Products.Where(p => p.Category.Contains(query.Category)).ToListAsync(cancellationToken);

            //mapping product entity to productdto
            var productDtos = products.ToDtoList();

            return new GetProductByCategoryResult(productDtos);
        }
    }

    public record GetProductByCategoryResponse(IEnumerable<ProductDto> Products);
    public class GetProductByCategoryEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products/category/{category}", async (string category, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByCategoryQuery(category));

                return Results.Ok(new GetProductByCategoryResponse(result.Products));
            })
            .WithName("GetProductByCategory")
            .Produces<GetProductByCategoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product By Category")
            .WithDescription("Get Product By Category");
        }
    }
}