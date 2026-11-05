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

    internal sealed class UpdateItemPriceInBasketHandler(IBasketRepository basketRepository) : ICommandHandler<UpdateItemPriceInBasketCommand, UpdateItemPriceInBasketResult>
    {
        public async Task<UpdateItemPriceInBasketResult> Handle(UpdateItemPriceInBasketCommand command, CancellationToken cancellationToken)
        {
            var baskets = await basketRepository.GetBasketsByProductId(command.ProductId, cancellationToken);

            var updatedBaskets = baskets
                .Where(basket => basket.UpdateItemPrice(command.ProductId, command.Price, command.PriceChangedAtUtc))
                .ToList();

            if (updatedBaskets.Count == 0)
                return new UpdateItemPriceInBasketResult(false);

            await basketRepository.SaveChangesAsync(cancellationToken: cancellationToken);

            return new UpdateItemPriceInBasketResult(true);
        }
    }
}
