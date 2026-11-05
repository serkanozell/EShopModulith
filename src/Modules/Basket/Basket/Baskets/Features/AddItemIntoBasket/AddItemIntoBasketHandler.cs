namespace Basket.Baskets.Features.AddItemIntoBasket
{
    public record AddItemIntoBasketCommand(string UserName, BasketItemDto BasketItemDto) : ICommand<AddItemIntoBasketResult>;
    public record AddItemIntoBasketResult(Guid Id);
    public class AddItemIntoBasketCommandValidator : AbstractValidator<AddItemIntoBasketCommand>
    {
        public AddItemIntoBasketCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");
            RuleFor(x => x.BasketItemDto.ProductId).NotEmpty().WithMessage("ProductId is required");
            RuleFor(x => x.BasketItemDto.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
        }
    }

    internal sealed class AddItemIntoBasketHandler(IBasketRepository repository, ISender sender) : ICommandHandler<AddItemIntoBasketCommand, AddItemIntoBasketResult>
    {
        public async Task<AddItemIntoBasketResult> Handle(AddItemIntoBasketCommand command, CancellationToken cancellationToken)
        {
            // Add shopping cart item into shopping cart
            var shoppingCart = await repository.GetBasket(command.UserName, false, cancellationToken);

            ///
            /// bu feature ileride catalog.contracts üzerinden çalışacak servis mantığına çevrilebilir.
            /// catalog.application katmanı catalog.contracts a referans verdiği için doğru bir yaklaşım olmayabilir.
            /// bunun yerine contracts içinde bir getproductbyid servisi ile kullanılabilir.
            ///
            var result = await sender.Send(new GetProductByIdQuery(command.BasketItemDto.ProductId), cancellationToken);

            shoppingCart.AddItem(command.BasketItemDto.ProductId,
                                 command.BasketItemDto.Quantity,
                                 command.BasketItemDto.Color,
                                 result.Product.Price,
                                 result.Product.Name);

            await repository.SaveChangesAsync(cancellationToken);

            return new AddItemIntoBasketResult(shoppingCart.Id.Value);
        }
    }

    public record AddItemIntoBasketRequest(string UserName, BasketItemDto BasketItemDto);
    public record AddItemIntoBasketResponse(Guid Id);

    public class AddItemIntoBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/basket/{userName}/items",
                async ([FromRoute] string userName,
                       [FromBody] AddItemIntoBasketRequest request,
                       ISender sender) =>
                {
                    var command = new AddItemIntoBasketCommand(userName, request.BasketItemDto);

                    var result = await sender.Send(command);

                    var response = new AddItemIntoBasketResponse(result.Id);

                    return Results.Created($"/basket/{response.Id}", response);
                })
            .Produces<AddItemIntoBasketResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Add Item Into Basket")
            .WithDescription("Add Item Into Basket")
            .RequireAuthorization();
        }
    }
}