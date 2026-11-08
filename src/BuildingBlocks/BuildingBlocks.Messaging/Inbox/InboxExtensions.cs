using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.Inbox
{
    public static class InboxExtensions
    {
        public static ModelBuilder ApplyInboxConfiguration(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
            return modelBuilder;
        }

        /// <summary>
        /// Verilen assembly'lerdeki InboxMessageHandler&lt;T&gt; sınıflarını bu context'in inbox'ına bağlar
        /// ve InboxProcessor'ı başlatır.
        /// </summary>
        public static IServiceCollection AddInboxProcessor<TContext>(this IServiceCollection services,
                                                                     IConfiguration configuration,
                                                                     params Assembly[] handlerAssemblies)
            where TContext : DbContext
        {
            services.AddOptions<InboxOptions>()
                    .Bind(configuration.GetSection(InboxOptions.SectionName));

            var registry = new InboxHandlerRegistry<TContext>();

            var handlerTypes = handlerAssemblies.SelectMany(a => a.GetTypes())
                                                .Where(t => t is { IsClass: true, IsAbstract: false }
                                                            && typeof(IInboxMessageHandler).IsAssignableFrom(t));

            foreach (var handlerType in handlerTypes)
            {
                var messageType = GetMessageType(handlerType);
                if (messageType is null) continue;

                registry.Add(messageType, handlerType);
                services.AddScoped(handlerType);
            }

            services.AddSingleton(registry);
            services.AddHostedService<InboxProcessor<TContext>>();

            return services;
        }

        private static Type? GetMessageType(Type handlerType)
        {
            for (var type = handlerType; type is not null; type = type.BaseType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(InboxMessageHandler<>))
                    return type.GetGenericArguments()[0];
            }

            return null;
        }
    }

    public sealed class InboxHandlerRegistry<TContext>
    {
        private readonly Dictionary<Type, List<Type>> _handlers = new();

        internal void Add(Type messageType, Type handlerType)
        {
            if (!_handlers.TryGetValue(messageType, out var list))
                _handlers[messageType] = list = [];

            list.Add(handlerType);
        }

        public IReadOnlyList<Type> GetHandlerTypes(Type messageType) =>
            _handlers.TryGetValue(messageType, out var list) ? list : [];
    }

    public sealed class InboxOptions
    {
        public const string SectionName = "Inbox";

        public bool Enabled { get; set; } = true;

        /// <summary>Okunacak mesaj kalmadığında (veya hata sonrası) bir sonraki yoklamaya kadar beklenecek süre.</summary>
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>Bu sayıya ulaşan mesajlar artık işlenmez (dead-letter gibi tabloda kalır).</summary>
        public int MaxRetryCount { get; set; } = 5;

        /// <summary>İşlenmiş kayıtların tutulacağı gün sayısı. Duplicate koruması bu pencere kadar geçerlidir.</summary>
        public int RetentionDays { get; set; } = 7;
    }
}
