using Basket.Baskets.Events;
using BuildingBlocks.Common.DDD;
using BuildingBlocks.Common.Dtos;

namespace Basket.Entities
{
    public class ShoppingCart : Aggregate<ShoppingCartId>
    {
        public UserName UserName { get; private set; } = default!;

        private readonly List<ShoppingCartItem> _items = new();
        public IReadOnlyList<ShoppingCartItem> Items => _items.AsReadOnly();

        public decimal TotalPrice => _items.Sum(i => i.Price.Value * i.Quantity.Value);

        private ShoppingCart(ShoppingCartId id, UserName userName)
        {
            Id = id;
            UserName = userName;
        }

        private ShoppingCart(UserName userName) : this(ShoppingCartId.Of(Guid.NewGuid()), userName)
        {
        }

        public static ShoppingCart Create(UserName userName)
        {
            ArgumentNullException.ThrowIfNull(userName);

            var cart = new ShoppingCart(userName);

            cart.AddDomainEvent(new BasketCreatedEvent(cart.Id.Value, cart.UserName.Value));

            return cart;
        }

        public void AddItem(Guid productId, int quantity, string color, decimal price, string productName)
        {
            var productIdValueObject = ProductId.Of(productId);
            var quantityValueObject = Quantity.Of(quantity);
            var colorValueObject = Color.Of(color);
            var priceValueObject = Price.Of(price);
            var productNameValueObject = ProductName.Of(productName);

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productIdValueObject && i.Color == colorValueObject);

            if (existingItem is not null)
                existingItem.IncreaseQuantity(quantityValueObject);
            else
                _items.Add(new ShoppingCartItem(Id, productIdValueObject, quantityValueObject, colorValueObject, priceValueObject, productNameValueObject));

            AddDomainEvent(new BasketItemAddedEvent(Id.Value, productIdValueObject.Value, quantityValueObject.Value, priceValueObject.Value, productNameValueObject.Value));
        }

        public void RemoveItem(Guid productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId.Value == productId);

            if (item is null)
                throw new InvalidOperationException($"Product '{productId}' is not in the cart.");

            _items.Remove(item);

            AddDomainEvent(new BasketItemRemovedEvent(Id.Value, productId));
        }

        public void UpdateItemQuantity(Guid productId, int quantity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

            var item = _items.FirstOrDefault(i => i.ProductId.Value == productId);

            if (item is null)
                throw new InvalidOperationException($"Product '{productId}' is not in the cart.");

            var quantityValueObject = Quantity.Of(quantity);

            if (item.Quantity == quantityValueObject)
                return;

            item.SetQuantity(quantityValueObject);
        }

        public bool UpdateItemPrice(Guid productId, decimal newPrice, DateTime priceChangedAtUtc)
        {
            var updated = false;

            foreach (var item in _items.Where(i => i.ProductId.Value == productId))
                updated |= item.UpdatePrice(newPrice, priceChangedAtUtc);

            return updated;
        }

        public void ClearItems() => _items.Clear();

        public void Activate()
        {
            IsActive = true;
            IsDeleted = false;
        }

        public void Checkout(
                             string UserName,
                             Guid CustomerId,
                             decimal TotalPrice,
                             string FirstName,
                             string LastName,
                             string EmailAddress,
                             string AddressLine,
                             string Country,
                             string State,
                             string ZipCode,
                             string CardName,
                             string CardNumber,
                             string Expiration,
                             string Cvv,
                             int PaymentMethod,
                             List<OrderItemDto> OrderItems)
        {
            if (!_items.Any())
                throw new InvalidOperationException("Cannot checkout an empty cart.");
            AddDomainEvent(new BasketCheckoutedEvent(Id.Value, UserName, CustomerId, TotalPrice, FirstName, LastName, EmailAddress, AddressLine, Country, State, ZipCode, CardName, CardNumber, Expiration, Cvv, PaymentMethod, OrderItems));
        }

        // Cache/persistence'tan geri yükleme için. Domain event üretmez.
        public static ShoppingCart Restore(ShoppingCartId id, UserName userName, IEnumerable<ShoppingCartItem> items)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(userName);
            ArgumentNullException.ThrowIfNull(items);

            var cart = new ShoppingCart(id, userName);
            cart._items.AddRange(items);
            return cart;
        }
    }
}
