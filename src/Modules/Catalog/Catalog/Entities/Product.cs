using BuildingBlocks.Common.DDD;
using Catalog.Products.Events;

namespace Catalog.Entities
{
    public class Product : Aggregate<ProductId>
    {
        public ProductName Name { get; private set; } = default!;
        public List<string> Category { get; private set; } = new();
        public Description? Description { get; private set; } = default!;
        public Price Price { get; private set; } = default!;
        public StockAvailability StockAvailability { get; private set; } = default!;
        public int StockQuantity => StockAvailability.AvailableQuantity;
        public bool IsInStock => StockAvailability.IsInStock;

        private Product()
        {
        }

        private Product(ProductName name, List<string> category, Description? description, Price price, int stockQuantity)
        {
            Id = ProductId.Of(Guid.NewGuid());
            Name = name;
            Category = category;
            Description = description;
            Price = price;
            StockAvailability = StockAvailability.FromAvailableQuantity(stockQuantity);
            IsActive = true;
            IsDeleted = false;
        }

        public static Product Create(ProductName name, decimal price, int stockQuantity, List<string> category, string? description = null)
        {
            ArgumentNullException.ThrowIfNull(name);
            var priceValueObject = Price.Of(price);
            var descriptionValueObject = Description.Of(description);
            ArgumentOutOfRangeException.ThrowIfNegative(stockQuantity);

            if (category == null || category.Count == 0)
                throw new ArgumentException("Category is required.", nameof(category));

            var product = new Product(name, category, descriptionValueObject, priceValueObject, stockQuantity);

            product.AddDomainEvent(new ProductCreatedEvent(product.Id,
                                                           product.Name,
                                                           product.Category,
                                                           product.Description?.Value,
                                                           product.Price.Value,
                                                           product.StockQuantity));

            return product;
        }

        public void UpdateDetails(ProductName name, string? description = null)
        {
            ArgumentNullException.ThrowIfNull(name);

            Name = name;
            Description = Description.Of(description);
        }

        public void ChangePrice(decimal price)
        {
            var priceValueObject = Price.Of(price);

            if (Price == priceValueObject)
                return;

            var oldPrice = Price.Value;
            Price = priceValueObject;

            AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, priceValueObject.Value));
        }

        public void ChangeCategory(List<string> category)
        {
            if (category == null || category.Count == 0)
                throw new ArgumentException("Category is required.", nameof(category));

            Category = category;
        }

        public void UpdateStockQuantity(bool isInStock, int quantity)
        {
            StockAvailability = new StockAvailability(quantity, isInStock);
        }

        public void Activate()
        {
            IsActive = true;
            IsDeleted = false;
        }

        public void Delete()
        {
            IsActive = false;
            IsDeleted = true;
        }
    }
}