namespace Basket.Baskets.Features.UpdateItemPriceInBasket
{
    public record UpdateItemPriceInBasketCommand(Guid ProductId, decimal Price, DateTime PriceChangedAtUtc) : ICommand<UpdateItemPriceInBasketResult>;
    public record UpdateItemPriceInBasketResult(bool IsSuccess);
    public class UpdateItemPriceInBasketCommandValidator : AbstractValidator<UpdateItemPriceInBasketCommand>
    {
        public UpdateItemPriceInBasketCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
            RuleFor(x => x.PriceChangedAtUtc).NotEmpty().WithMessage("PriceChangedAtUtc is required");
        }
    }

    internal sealed class UpdateItemPriceInBasketHandler(BasketDbContext basketDbContext) : ICommandHandler<UpdateItemPriceInBasketCommand, UpdateItemPriceInBasketResult>
    {
        public async Task<UpdateItemPriceInBasketResult> Handle(UpdateItemPriceInBasketCommand command, CancellationToken cancellationToken)
        {
            var itemsToUpdate = await basketDbContext.ShoppingCartItems
                  .Where(x => x.ProductId == ProductId.Of(command.ProductId))
                  .ToListAsync(cancellationToken);

            if (itemsToUpdate.Count == 0)
                return new UpdateItemPriceInBasketResult(false);

            foreach (var item in itemsToUpdate)
            {
                item.UpdatePrice(command.Price, command.PriceChangedAtUtc);
            }

            await basketDbContext.SaveChangesAsync(cancellationToken);

            return new UpdateItemPriceInBasketResult(true);
        }
    }
}
