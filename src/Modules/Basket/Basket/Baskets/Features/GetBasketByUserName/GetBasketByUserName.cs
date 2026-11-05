using Basket.Baskets.Exceptions;

namespace Basket.Baskets.Features.GetBasketByUsername
{
    public record GetBasketByUserNameQuery(string UserName) : IQuery<GetBasketByUserNameResult>;

    public record GetBasketByUserNameResult(BasketDto Basket);

    internal sealed class GetBasketByUserNameQueryHandler(IBasketRepository repository) : IQueryHandler<GetBasketByUserNameQuery, GetBasketByUserNameResult>
    {
        public async Task<GetBasketByUserNameResult> Handle(GetBasketByUserNameQuery query, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query.UserName);

            var basket = await repository.GetBasket(query.UserName, true, cancellationToken)
                         ?? throw new BasketNotFoundException($"Basket for '{query.UserName}' was not found.");

            return new GetBasketByUserNameResult(basket.ToDto());
        }
    }

    public record GetBasketByUserNameResponse(BasketDto Basket);

    public class GetBasketByUserNameEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/baskets/{userName}", async (string userName, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetBasketByUserNameQuery(userName), cancellationToken);

                var response = new GetBasketByUserNameResponse(result.Basket);

                return Results.Ok(response);
            })
            .WithName("GetBasketByUserName")
            .WithTags("Baskets")
            .WithSummary("Get Basket")
            .WithDescription("Get Basket")
            .Produces<GetBasketByUserNameResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}