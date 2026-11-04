using BuildingBlocks.Common.Data;
using BuildingBlocks.Messaging.Outbox;

namespace Catalog.Data
{
    public interface ICatalogDbContext : IApplicationDbContext
    {
        DbSet<Product> Products { get; }
        DbSet<OutboxMessage> OutboxMessages { get; }
    }

    public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options), ICatalogDbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("catalog");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

            modelBuilder.ApplyOutboxConfiguration();

            base.OnModelCreating(modelBuilder);
        }
    }
}