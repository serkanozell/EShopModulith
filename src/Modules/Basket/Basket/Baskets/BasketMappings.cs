using Basket.Baskets.Dtos;

namespace Basket.Baskets
{
    public static class BasketMappings
    {
        public static BasketDto ToDto(this ShoppingCart cart) =>
            new(cart.Id.Value,
                cart.UserName.Value,
                cart.Items.Select(i => i.ToDto())
                          .ToList());

        public static BasketItemDto ToDto(this ShoppingCartItem item) =>
            new(
                item.Id.Value,
                item.ShoppingCartId.Value,
                item.ProductId.Value,
                item.ProductName.Value,
                item.Color.Value,
                item.Quantity.Value,
                item.Price.Value);
    }
}