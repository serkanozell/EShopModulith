namespace Catalog.ValueObjects
{
    public sealed record StockAvailability
    {
        public int AvailableQuantity { get; }
        public bool IsInStock { get; }

        public StockAvailability(int availableQuantity, bool isInStock)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(availableQuantity);

            if (isInStock != (availableQuantity > 0))
                throw new ArgumentException("IsInStock must match whether available quantity is greater than zero.", nameof(isInStock));

            AvailableQuantity = availableQuantity;
            IsInStock = isInStock;
        }

        public static StockAvailability FromAvailableQuantity(int availableQuantity) =>
            new(availableQuantity, availableQuantity > 0);
    }
}