namespace Basket.Data.Repositories
{
    public interface IBasketRepository
    {
        Task<ShoppingCart> GetBasket(string userName, bool asNoTracking = true, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ShoppingCart>> GetBasketsByProductId(Guid productId, CancellationToken cancellationToken = default);
        Task<ShoppingCart> CreateBasket(ShoppingCart basket, CancellationToken cancellationToken = default);
        Task DeleteBasket(string userName, CancellationToken cancellationToken = default);   // kaydetmez, SaveChangesAsync bekler
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}