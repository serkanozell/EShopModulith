namespace Basket.ValueObjects
{
    public record ProductId
    {
        public Guid Value { get; }

        private ProductId(Guid value) => Value = value;

        public static ProductId Of(Guid value)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(value, Guid.Empty);

            return new ProductId(value);
        }
    }
}