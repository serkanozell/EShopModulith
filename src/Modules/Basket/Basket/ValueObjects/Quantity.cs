namespace Basket.ValueObjects
{
    public record Quantity
    {
        public int Value { get; }

        private Quantity(int value) => Value = value;

        public static Quantity Of(int value)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

            return new Quantity(value);
        }

        public Quantity Increase(Quantity quantity)
        {
            ArgumentNullException.ThrowIfNull(quantity);

            return Of(checked(Value + quantity.Value));
        }
    }
}