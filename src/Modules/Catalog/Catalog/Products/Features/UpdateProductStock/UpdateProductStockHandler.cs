using Catalog.Contracts.Features;

namespace Catalog.Products.Features.UpdateProductStock
{
    internal sealed class UpdateProductStockCommandHandler(ICatalogDbContext catalogDbContext) : ICommandHandler<UpdateProductStockCommand, UpdateProductStockCommandResult>
    {
        public async Task<UpdateProductStockCommandResult> Handle(UpdateProductStockCommand command, CancellationToken cancellationToken)
        {
            var product = await catalogDbContext.Products.FirstOrDefaultAsync(p => p.Id == ProductId.Of(command.ProductId), cancellationToken) ?? throw new KeyNotFoundException($"Product with id '{command.ProductId}' was not found.");

            product.UpdateStockQuantity(command.IsInStock, command.AvailableQuantity);

            catalogDbContext.Products.Update(product);

            await catalogDbContext.SaveChangesAsync(cancellationToken);

            return new UpdateProductStockCommandResult(true);
        }
    }
}