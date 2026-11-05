namespace Basket.Baskets.Features.DeleteBasket
{
    public record DeleteBasketCommand(string UserName) : ICommand<DeleteBasketResult>;

    public record DeleteBasketResult(bool IsSuccess);

    public class DeleteBasketCommandValidator : AbstractValidator<DeleteBasketCommand>
    {
        public DeleteBasketCommandValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("UserName is required.");
        }
    }

    internal sealed class DeleteBasketCommandHandler(IBasketRepository repository) : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
    {
        public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
        {
            await repository.DeleteBasket(command.UserName, cancellationToken);

            return new DeleteBasketResult(true);
        }
    }

    public record DeleteBasketResponse(bool IsSuccess);

    public class DeleteBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/baskets/{userName}", async (string userName, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new DeleteBasketCommand(userName), cancellationToken);

                var response = new DeleteBasketResponse(result.IsSuccess);

                return Results.Ok(response);
            })
            .WithName("DeleteBasket")
            .WithTags("Baskets")
            .WithSummary("Delete Basket")
            .WithDescription("Delete Basket")
            .Produces<DeleteBasketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}