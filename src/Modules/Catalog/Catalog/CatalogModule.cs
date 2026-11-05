using BuildingBlocks.Common.Data;
using BuildingBlocks.Common.Interceptors;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Catalog
{
    public static class CatalogModule
    {
        public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Register services, repositories, and other dependencies here
            // Example:
            // services.AddScoped<ICatalogService, CatalogService>();
            // services.AddScoped<ICatalogRepository, CatalogRepository>();

            // MediatR, behavior ve validator kayıtları host tarafında
            // AddMediatRWithAssemblies ile merkezi olarak yapılır.

            // Data - Infrastructure services
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, AuditableEntityInterceptor>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>());

            services.AddScoped<ICatalogDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());

            services.AddDbContext<CatalogDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString)
                       .UseSnakeCaseNamingConvention();
            });

            services.AddOutboxProcessor<CatalogDbContext>(configuration);

            return services;
        }

        public static IApplicationBuilder UseCatalogModule(this IApplicationBuilder app)
        {
            // Configure middleware, routing, etc. here
            // Example:
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.UseMigrations<CatalogDbContext>();

            return app;
        }
    }
}