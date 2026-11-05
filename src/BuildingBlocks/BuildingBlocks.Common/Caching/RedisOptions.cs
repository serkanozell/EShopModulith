using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BuildingBlocks.Common.Caching
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int DefaultAbsoluteExpirationInMinutes { get; set; } = 60;
        public int DefaultSlidingExpirationInMinutes { get; set; } = 30;
    }

    public sealed class RedisOptionsValidator : IValidateOptions<RedisOptions>
    {
        public ValidateOptionsResult Validate(string? name, RedisOptions options)
        {
            var failures = new List<string>();

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                failures.Add($"{nameof(RedisOptions.ConnectionString)} is required.");
            }
            else
            {
                try
                {
                    ConfigurationOptions.Parse(options.ConnectionString);
                }
                catch (Exception ex)
                {
                    failures.Add($"{nameof(RedisOptions.ConnectionString)} is invalid: {ex.Message}");
                }
            }

            return failures.Count > 0
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}