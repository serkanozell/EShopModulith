using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.Property(p => p.Id)
                   .HasConversion(id => id.Value, value => ProductId.Of(value));

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .HasConversion(name => name.Value, value => ProductName.Of(value))
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Category)
                   .IsRequired();


            builder.Property(p => p.Description)
                   .HasConversion(description => description!.Value, value => Description.Of(value))
                   .HasMaxLength(2000);

            builder.Property(p => p.Price)
                   .IsRequired()
                   .HasConversion(price => price.Value, value => Price.Of(value))
                   .HasColumnType("decimal(18,2)");

            builder.OwnsOne(p => p.StockAvailability, stockAvailability =>
            {
                stockAvailability.Property(stock => stock.AvailableQuantity)
                                 .HasColumnName("stock_quantity")
                                 .IsRequired();

                stockAvailability.Property(stock => stock.IsInStock)
                                 .HasColumnName("is_in_stock")
                                 .IsRequired();
            });

            builder.Navigation(p => p.StockAvailability)
                   .IsRequired();

            builder.Ignore(p => p.StockQuantity);
            builder.Ignore(p => p.IsInStock);

        }
    }
}