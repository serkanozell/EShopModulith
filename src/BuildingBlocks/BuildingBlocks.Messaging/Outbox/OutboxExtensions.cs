using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.Outbox
{
    public static class OutboxExtensions
    {
        public static ModelBuilder ApplyOutboxConfiguration(
            this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

            return modelBuilder;
        }
    }

    public static class OutboxProcessorExtensions
    {
        public static IServiceCollection AddOutboxProcessor<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            services.AddOptions<OutboxOptions>()
                    .Bind(configuration.GetSection(OutboxOptions.SectionName));

            services.AddHostedService<OutboxProcessor<TContext>>();

            return services;
        }
    }

    public sealed class OutboxOptions
    {
        public const string SectionName = "Outbox";

        public bool Enabled { get; set; } = true;

        /// <summary>Tek turda işlenecek maksimum mesaj sayısı.</summary>
        public int BatchSize { get; set; } = 20;

        /// <summary>Okunacak mesaj kalmadığında bir sonraki yoklamaya kadar beklenecek süre.</summary>
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>Bu sayıya ulaşan mesajlar artık işlenmez (dead-letter gibi tabloda kalır).</summary>
        public int MaxRetryCount { get; set; } = 5;
    }
}