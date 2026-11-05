using Basket.Baskets.Exceptions;

namespace Basket.Data.Repositories
{
    public class BasketRepository(BasketDbContext dbContext) : IBasketRepository
    {
        public async Task<ShoppingCart> GetBasket(string userName, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = dbContext.ShoppingCarts
                                 .Include(x => x.Items)
                                 .Where(x => x.UserName == UserName.Of(userName) && x.IsActive && !x.IsDeleted);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.SingleOrDefaultAsync(cancellationToken)
                   ?? throw new BasketNotFoundException(userName);
        }

        public async Task<IReadOnlyList<ShoppingCart>> GetBasketsByProductId(Guid productId, CancellationToken cancellationToken = default)
        {
            var id = ProductId.Of(productId);

            return await dbContext.ShoppingCarts
                                  .Include(x => x.Items)
                                  .Where(x => x.IsActive && !x.IsDeleted && x.Items.Any(i => i.ProductId == id))
                                  .ToListAsync(cancellationToken);
        }

        public async Task<ShoppingCart> CreateBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
        {
            dbContext.ShoppingCarts.Add(basket);
            await dbContext.SaveChangesAsync(cancellationToken);
            return basket;
        }

        public async Task DeleteBasket(string userName, CancellationToken cancellationToken = default)
        {
            var basket = await GetBasket(userName, asNoTracking: false, cancellationToken);
            dbContext.ShoppingCarts.Remove(basket);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => dbContext.SaveChangesAsync(cancellationToken);
    }
}