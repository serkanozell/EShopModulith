namespace Catalog.ValueObjects
{
    public record Description
    {
        public string Value { get; }

        private Description(string value) => Value = value;

        public static Description? Of(string? value)
        {
            if (value is null)
                return null;

            if (value.Length > 500)
                throw new ArgumentOutOfRangeException(nameof(value), "Description cannot exceed 500 characters.");

            return new Description(value);
        }
    }
}