namespace Basket.Baskets.Dtos
{
    public record CreateBasketDto(string UserName,
                                   List<CreateBasketItemDto> Items);

    public record CreateBasketItemDto(Guid ProductId,
                                       string Color,
                                       int Quantity);
}