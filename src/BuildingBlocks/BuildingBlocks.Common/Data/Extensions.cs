using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Common.Data
{
    public static class Extensions
    {
        public static IApplicationBuilder UseMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            MigrateDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();

            return app;
        }

        private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider) where TContext : DbContext
        {
            var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}