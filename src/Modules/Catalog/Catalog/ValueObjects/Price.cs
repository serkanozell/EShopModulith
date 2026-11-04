namespace Catalog.ValueObjects
{
    public record Price
    {
        public decimal Value { get; }

        private Price(decimal value) => Value = value;

        public static Price Of(decimal value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            return new Price(value);
        }
    }
}