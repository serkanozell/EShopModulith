namespace Basket.Data
{
    public class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
    {
        public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
        public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("basket");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BasketDbContext).Assembly);

            modelBuilder.ApplyOutboxConfiguration();

            modelBuilder.ApplyInboxConfiguration();

            base.OnModelCreating(modelBuilder);
        }
    }
}