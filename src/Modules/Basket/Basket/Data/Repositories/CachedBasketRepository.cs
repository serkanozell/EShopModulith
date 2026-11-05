using Basket.Helpers;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Data.Repositories
{
    // Scoped: her request / consumer mesajı için kendi "dirty" kümesi vardır.
    public sealed class CachedBasketRepository(IBasketRepository basketRepository, IDistributedCache cache) : IBasketRepository
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new ShoppingCartConverter(), new ShoppingCartItemConverter() }
        };

        // Gecikmiş/yarışan bir okuma bayat veri yazsa bile en geç 30 dk sonra düzelir.
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        private readonly HashSet<string> _dirtyUserNames = new(StringComparer.Ordinal);

        private static string Key(string userName) => $"basket:{userName}";

        public async Task<ShoppingCart> GetBasket(string userName, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            // Tracked okuma = değiştirme niyeti. Cache'e bakma, SaveChanges sonrası temizlenecek.
            if (!asNoTracking)
            {
                _dirtyUserNames.Add(userName);
                return await basketRepository.GetBasket(userName, asNoTracking: false, cancellationToken);
            }

            var cached = await cache.GetStringAsync(Key(userName), cancellationToken);
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<ShoppingCart>(cached, JsonOptions)!;

            var basket = await basketRepository.GetBasket(userName, asNoTracking: true, cancellationToken);

            await cache.SetStringAsync(Key(userName), JsonSerializer.Serialize(basket, JsonOptions), CacheOptions, cancellationToken);

            return basket;
        }

        // Cache'e hiç dokunmaz; tracked döndüğü için etkilenen kullanıcılar dirty'ye eklenir.
        public async Task<IReadOnlyList<ShoppingCart>> GetBasketsByProductId(Guid productId, CancellationToken cancellationToken = default)
        {
            var baskets = await basketRepository.GetBasketsByProductId(productId, cancellationToken);

            foreach (var basket in baskets)
                _dirtyUserNames.Add(basket.UserName.Value);

            return baskets;
        }

        public async Task<ShoppingCart> CreateBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
        {
            var created = await basketRepository.CreateBasket(basket, cancellationToken);

            // basketRepository zaten kaydetti, bu yüzden doğrudan temizliyoruz.
            await cache.RemoveAsync(Key(basket.UserName.Value), CancellationToken.None);

            return created;
        }

        public async Task DeleteBasket(string userName, CancellationToken cancellationToken = default)
        {
            _dirtyUserNames.Add(userName);
            await basketRepository.DeleteBasket(userName, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await basketRepository.SaveChangesAsync(cancellationToken);

            // Commit bittikten SONRA temizle. Token verilmez, çünkü commit sonrası iptal edilirse key'ler cache'te kalırdı.
            await InvalidateAsync();

            return result;
        }

        private async Task InvalidateAsync()
        {
            if (_dirtyUserNames.Count == 0) return;

            var userNames = _dirtyUserNames.ToArray();
            _dirtyUserNames.Clear();

            await Task.WhenAll(userNames.Select(u => cache.RemoveAsync(Key(u), CancellationToken.None)));
        }
    }
}