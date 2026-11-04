namespace Catalog.ValueObjects
{
    public record ProductName
    {
        public string Value { get; }

        private ProductName(string value) => Value = value;

        public static ProductName Of(string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (value.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "ProductName cannot exceed 100 characters.");
            }

            return new ProductName(value);
        }
    }
}