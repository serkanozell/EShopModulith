namespace Basket.Data
{
    public class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
    {
        public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
        public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("basket");

            builder.ApplyConfigurationsFromAssembly(typeof(BasketDbContext).Assembly);

            builder.ApplyOutboxConfiguration();

            base.OnModelCreating(builder);
        }
    }
}