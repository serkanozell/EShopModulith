using BuildingBlocks.Common.DDD;

namespace Basket.Entities
{
    public class ShoppingCartItem : Entity<ShoppingCartItemId>
    {
        public ShoppingCartId ShoppingCartId { get; private set; } = default!;
        public ProductId ProductId { get; private set; } = default!;
        public Quantity Quantity { get; private set; } = default!;
        public Color Color { get; private set; } = default!;
        public Price Price { get; private set; } = default!;
        public ProductName ProductName { get; private set; } = default!;
        public DateTime PriceUpdatedAtUtc { get; private set; }

        private ShoppingCartItem()
        {
        }

        internal ShoppingCartItem(ShoppingCartId shoppingCartId, ProductId productId, Quantity quantity, Color color, Price price, ProductName productName)
        {
            Id = ShoppingCartItemId.Of(Guid.NewGuid());
            ShoppingCartId = shoppingCartId;
            ProductId = productId;
            Quantity = quantity;
            Color = color;
            Price = price;
            ProductName = productName;
            PriceUpdatedAtUtc = DateTime.UtcNow;
            IsActive = true;
            IsDeleted = false;
        }

        [JsonConstructor]
        public ShoppingCartItem(Guid id, Guid shoppingCartId, Guid productId, int quantity, string color, decimal price, string productName)
        {
            Id = ShoppingCartItemId.Of(id);
            ShoppingCartId = ShoppingCartId.Of(shoppingCartId);
            ProductId = ProductId.Of(productId);
            Quantity = Quantity.Of(quantity);
            Color = Color.Of(color);
            Price = Price.Of(price);
            ProductName = ProductName.Of(productName);
            PriceUpdatedAtUtc = DateTime.UtcNow;
            IsActive = true;
            IsDeleted = false;
        }

        public bool UpdatePrice(decimal newPrice, DateTime priceChangedAtUtc)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newPrice);

            if (priceChangedAtUtc <= PriceUpdatedAtUtc)
                return false;

            Price = Price.Of(newPrice);
            PriceUpdatedAtUtc = priceChangedAtUtc;
            return true;
        }

        internal void IncreaseQuantity(Quantity quantity)
        {
            Quantity = Quantity.Increase(quantity);
        }

        internal void SetQuantity(Quantity quantity)
        {
            ArgumentNullException.ThrowIfNull(quantity);
            Quantity = quantity;
        }
    }
}
