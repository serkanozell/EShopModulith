namespace BuildingBlocks.Common.Dtos
{
    public record OrderItemDto(Guid ProductId, int Quantity, decimal Price);
}