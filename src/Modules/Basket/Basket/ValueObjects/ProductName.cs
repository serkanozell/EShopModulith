namespace Basket.ValueObjects
{
    public record ProductName
    {
        public string Value { get; }

        private ProductName(string value) => Value = value;

        public static ProductName Of(string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (value.Length > 200)
                throw new ArgumentOutOfRangeException(nameof(value), "ProductName cannot exceed 200 characters.");

            return new ProductName(value);
        }
    }
}