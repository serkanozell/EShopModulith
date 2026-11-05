namespace Basket.Data.Configuration
{
    public class BasketItemConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
        {
            builder.Property(i => i.Id)
                   .HasConversion(id => id.Value, value => ShoppingCartItemId.Of(value));

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ShoppingCartId)
                   .HasConversion(id => id.Value, value => ShoppingCartId.Of(value))
                   .IsRequired();

            builder.Property(i => i.ProductId)
                   .HasConversion(id => id.Value, value => ProductId.Of(value))
                   .IsRequired();

            builder.Property(i => i.Quantity)
                   .HasConversion(quantity => quantity.Value, value => Quantity.Of(value))
                   .IsRequired();

            builder.Property(i => i.Color)
                   .HasConversion(color => color.Value, value => Color.Of(value))
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(i => i.Price)
                   .IsRequired()
                   .HasConversion(price => price.Value, value => Price.Of(value))
                   .HasColumnType("decimal(18,2)");

            builder.Property(i => i.ProductName)
                   .HasConversion(name => name.Value, value => ProductName.Of(value))
                   .IsRequired()
                   .HasMaxLength(200);
        }
    }
}