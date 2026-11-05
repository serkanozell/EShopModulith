using Basket.Baskets.Exceptions;
using BuildingBlocks.Common.Dtos;
using MassTransit;

namespace Basket.Baskets.Features.CheckoutBasket
{
    public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckout) : ICommand<CheckoutBasketResult>;
    public record CheckoutBasketResult(bool IsSuccess);
    public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
    {
        public CheckoutBasketCommandValidator()
        {
            RuleFor(x => x.BasketCheckout).NotNull().WithMessage("BasketCheckoutDto can't be null");
            RuleFor(x => x.BasketCheckout.UserName).NotEmpty().WithMessage("UserName is required");
        }
    }

    internal sealed class CheckoutBasketHandler(IBasketRepository basketRepository, IPublishEndpoint publishEndpoint, ISender sender) : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
    {
        public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
        {
            var checkout = command.BasketCheckout;

            var basket = await basketRepository.GetBasket(checkout.UserName, asNoTracking: false, cancellationToken: cancellationToken)
                ?? throw new BasketNotFoundException(checkout.UserName);

            // Prices are re-validated against catalog so that the order is always created with current prices
            foreach (var productId in basket.Items.Select(i => i.ProductId.Value).Distinct().ToList())
            {
                var result = await sender.Send(new GetProductByIdQuery(productId), cancellationToken);

                basket.UpdateItemPrice(productId, result.Product.Price, DateTime.UtcNow);
            }

            // kontrol sonrası totalprice değiştiyse kullanıcıya bilgi verilmeli.
            // tüm id leri distinct alıp hepsini tek seferde getiren servis yapılabilir.

            basket.Checkout(checkout.UserName,
                            checkout.CustomerId,
                            basket.TotalPrice,
                            checkout.FirstName,
                            checkout.LastName,
                            checkout.EmailAddress,
                            checkout.AddressLine,
                            checkout.Country,
                            checkout.State,
                            checkout.ZipCode,
                            checkout.CardName,
                            checkout.CardNumber,
                            checkout.Expiration,
                            checkout.Cvv,
                            checkout.PaymentMethod,
                            basket.Items.Select(i => new OrderItemDto(i.ProductId.Value, i.Quantity.Value, i.Price.Value)).ToList());

            await basketRepository.DeleteBasket(checkout.UserName, cancellationToken);

            await basketRepository.SaveChangesAsync(cancellationToken: cancellationToken);

            return new CheckoutBasketResult(true);
        }
    }

    public record CheckoutBasketRequest(BasketCheckoutDto BasketCheckout);
    public record CheckoutBasketResponse(bool IsSuccess);

    public class CheckoutBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/basket/checkout",
                async (CheckoutBasketRequest request, ISender sender) =>
                {
                    var result = await sender.Send(new CheckoutBasketCommand(request.BasketCheckout));

                    return Results.Ok(new CheckoutBasketResponse(result.IsSuccess));
                })
            .WithName("CheckoutBasket")
            .Produces<CheckoutBasketResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Checkout Basket")
            .WithDescription("Checkout Basket");
            //.RequireAuthorization();
        }
    }
}