using Catalog.Contracts.Features;

namespace Catalog.Products
{
    public static class ProductMappings
    {
        public static ProductDto ToDto(this Product product) =>
            new(product.Id.Value,
                product.Name.Value,
                product.Category,
                product.Description?.Value,
                product.Price.Value,
                product.StockAvailability.AvailableQuantity,
                product.StockAvailability.IsInStock,
                product.IsActive,
                product.CreatedAt,
                product.UpdatedAt);

        public static List<ProductDto> ToDtoList(this IEnumerable<Product> products) =>
            products.Select(p => p.ToDto()).ToList();
    }
}