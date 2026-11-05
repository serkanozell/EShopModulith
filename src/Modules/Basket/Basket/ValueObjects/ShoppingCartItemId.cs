namespace Basket.ValueObjects
{
    public record ShoppingCartItemId
    {
        public Guid Value { get; }

        private ShoppingCartItemId(Guid value) => Value = value;

        public static ShoppingCartItemId Of(Guid value)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(value, Guid.Empty);

            return new ShoppingCartItemId(value);
        }
    }
}