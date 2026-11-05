using BuildingBlocks.Common.Caching;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Common.Extensions
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<RedisOptions>, RedisOptionsValidator>());

            services.AddOptions<RedisOptions>()
                    .Bind(configuration.GetSection("RedisOptions"))
                    .ValidateOnStart();

            // RedisCacheOptions, doğrulanmış RedisOptions'tan beslenir.
            services.AddStackExchangeRedisCache(_ => { });

            services.AddOptions<RedisCacheOptions>()
                    .Configure<IOptions<RedisOptions>>((cacheOptions, redisOptions) =>
                    {
                        cacheOptions.Configuration = redisOptions.Value.ConnectionString;
                    });

            return services;
        }
    }
}