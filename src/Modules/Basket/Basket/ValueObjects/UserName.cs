namespace Basket.ValueObjects
{
    public record UserName
    {
        public string Value { get; }

        private UserName(string value) => Value = value;

        public static UserName Of(string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (value.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "UserName cannot exceed 100 characters.");
            }

            return new UserName(value);
        }
    }
}