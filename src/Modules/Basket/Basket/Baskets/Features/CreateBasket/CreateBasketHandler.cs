namespace Basket.Baskets.Features.CreateBasket
{
    public record CreateBasketCommand(CreateBasketDto CreateBasketDto) : ICommand<CreateBasketResult>;

    public record CreateBasketResult(Guid Id);

    public class CreateBasketCommandValidator : AbstractValidator<CreateBasketCommand>
    {
        public CreateBasketCommandValidator()
        {
            RuleFor(x => x.CreateBasketDto.UserName)
                .NotEmpty()
                .WithMessage("UserName is required.")
                .MaximumLength(100)
                .WithMessage("UserName must not exceed 100 characters.");
        }
    }

    internal sealed class CreateBasketCommandHandler(IBasketRepository repository, ISender sender) : ICommandHandler<CreateBasketCommand, CreateBasketResult>
    {
        public async Task<CreateBasketResult> Handle(CreateBasketCommand command, CancellationToken cancellationToken)
        {
            var basket = await CreateNewBasket(command.CreateBasketDto, cancellationToken);

            await repository.CreateBasket(basket, cancellationToken);

            return new CreateBasketResult(basket.Id.Value);
        }

        private async Task<ShoppingCart> CreateNewBasket(CreateBasketDto createBasketDto, CancellationToken cancellationToken)
        {
            // create new basket
            var newBasket = ShoppingCart.Create(UserName.Of(createBasketDto.UserName));

            foreach (var item in createBasketDto.Items)
            {
                // price and name are always resolved from catalog, never trusted from client input
                var result = await sender.Send(new GetProductByIdQuery(item.ProductId), cancellationToken);

                newBasket.AddItem(item.ProductId,
                                  item.Quantity,
                                  item.Color,
                                  result.Product.Price,
                                  result.Product.Name);
            }

            return newBasket;
        }
    }

    public record CreateBasketRequest(CreateBasketDto CreateBasketDto);

    public record CreateBasketResponse(Guid Id);

    public class CreateBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/baskets", async (CreateBasketRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CreateBasketCommand(request.CreateBasketDto);

                var result = await sender.Send(command, cancellationToken);

                var response = new CreateBasketResponse(result.Id);

                return Results.Created($"/api/baskets/{response.Id}", response);
            })
            .WithName("Create Basket")
            .WithTags("Baskets")
            .WithSummary("Create Basket")
            .WithDescription("Create Basket")
            .Produces<CreateBasketResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        }
    }
}