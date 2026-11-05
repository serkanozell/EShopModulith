namespace Basket.ValueObjects
{
    public record Color
    {
        public string Value { get; }

        private Color(string value) => Value = value;

        public static Color Of(string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (value.Length > 50)
                throw new ArgumentOutOfRangeException(nameof(value), "Color cannot exceed 50 characters.");

            return new Color(value);
        }
    }
}