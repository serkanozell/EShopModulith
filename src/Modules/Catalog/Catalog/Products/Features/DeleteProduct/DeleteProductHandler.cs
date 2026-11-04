namespace Catalog.Products.Features.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResult>;

    public record DeleteProductResult(bool IsSuccess);

    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
        }
    }

    internal sealed class DeleteProductCommandHandler(ICatalogDbContext catalogDbContext) : ICommandHandler<DeleteProductCommand, DeleteProductResult>
    {
        public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            var product = await catalogDbContext.Products.FirstOrDefaultAsync(p => p.Id == ProductId.Of(command.Id), cancellationToken)
                ?? throw new KeyNotFoundException($"Product with id '{command.Id}' was not found.");

            catalogDbContext.Products.Remove(product);

            await catalogDbContext.SaveChangesAsync(cancellationToken);

            return new DeleteProductResult(true);
        }
    }

    public record DeleteProductResponse(bool IsSuccess);

    public class DeleteProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);

                return Results.Ok(new DeleteProductResponse(result.IsSuccess));
            })
            .WithName("DeleteProduct")
            .WithTags("Products")
            .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
            //.RequireAuthorization();
        }
    }
}