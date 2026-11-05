namespace Basket.Data.Configuration
{
    public class BasketConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
            builder.Property(c => c.Id)
                   .HasConversion(id => id.Value, value => ShoppingCartId.Of(value));

            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserName)
                   .HasConversion(userName => userName.Value, value => UserName.Of(value))
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(c => c.UserName);

            builder.Ignore(c => c.TotalPrice);

            builder.HasMany(c => c.Items)
                   .WithOne()
                   .HasForeignKey(i => i.ShoppingCartId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}