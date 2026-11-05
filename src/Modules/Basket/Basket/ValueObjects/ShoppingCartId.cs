namespace Basket.ValueObjects
{
    public record ShoppingCartId
    {
        public Guid Value { get; }

        private ShoppingCartId(Guid value) => Value = value;

        public static ShoppingCartId Of(Guid value)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(value, Guid.Empty);

            return new ShoppingCartId(value);
        }
    }
}