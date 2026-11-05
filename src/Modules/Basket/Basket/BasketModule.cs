namespace Basket
{
    public static class BasketModule
    {
        public static IServiceCollection AddBasketModule(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add services to the container.
            // 1. Api Endpoint services

            // 2. Application Use Case services
            services.AddScoped<IBasketRepository, BasketRepository>();
            services.Decorate<IBasketRepository, CachedBasketRepository>();

            // 3. Data - Infrastructure services
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, AuditableEntityInterceptor>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>());

            services.AddDbContext<BasketDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString)
                       .UseSnakeCaseNamingConvention();
            });

            services.AddOutboxProcessor<BasketDbContext>(configuration);

            return services;
        }

        public static IApplicationBuilder UseBasketModule(this IApplicationBuilder app)
        {
            // Configure the HTTP request pipeline.
            // 1. Use Api Endpoint services

            // 2. Use Application Use Case services

            // 3. Use Data - Infrastructure services
            app.UseMigrations<BasketDbContext>();

            return app;
        }
    }
}