namespace Catalog.Contracts.Features
{
    public record ProductDto(Guid Id,
                             string Name,
                             List<string> Category,
                             string? Description,
                             decimal Price,
                             int StockQuantity,
                             bool IsInStock,
                             bool IsActive,
                             DateTime? CreatedAt,
                             DateTime? UpdatedAt);
}