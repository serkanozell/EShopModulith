namespace Catalog.Products.Features.UpdateProduct
{
    public record UpdateProductCommand(Guid Id,
                                   string Name,
                                   List<string> Category,
                                   string? Description,
                                   decimal Price) : ICommand<UpdateProductResult>;

    public record UpdateProductResult(bool IsSuccess);

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Category is required.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero.");
        }
    }

    internal sealed class UpdateProductCommandHandler(ICatalogDbContext catalogDbContext) : ICommandHandler<UpdateProductCommand, UpdateProductResult>
    {
        public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            var product = await catalogDbContext.Products.FirstOrDefaultAsync(p => p.Id == ProductId.Of(command.Id), cancellationToken)
                ?? throw new KeyNotFoundException($"Product with id '{command.Id}' was not found.");

            product.UpdateDetails(ProductName.Of(command.Name), command.Description);
            product.ChangeCategory(command.Category);
            product.ChangePrice(command.Price);

            catalogDbContext.Products.Update(product);

            await catalogDbContext.SaveChangesAsync(cancellationToken);

            return new UpdateProductResult(true);
        }
    }

    public record UpdateProductRequest(string Name,
                                       List<string> Category,
                                       string? Description,
                                       decimal Price);

    public record UpdateProductResponse(bool IsSuccess);

    public class UpdateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new UpdateProductCommand(id,
                                                       request.Name,
                                                       request.Category,
                                                       request.Description,
                                                       request.Price);

                var result = await sender.Send(command, cancellationToken);

                return Results.Ok(new UpdateProductResponse(result.IsSuccess));
            })
            .WithName("UpdateProduct")
            .WithTags("Products")
            .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        }
    }
}