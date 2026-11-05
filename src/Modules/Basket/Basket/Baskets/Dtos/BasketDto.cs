namespace Basket.Baskets.Dtos
{
    public record BasketDto(Guid Id,
                            string UserName,
                            List<BasketItemDto> Items);

    public record BasketItemDto(Guid Id,
                                Guid ShoppingCartId,
                                Guid ProductId,
                                string ProductName,
                                string Color,
                                int Quantity,
                                decimal Price);
}