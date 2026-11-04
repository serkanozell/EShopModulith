namespace Catalog.Products.Features.CreateProduct
{
    public record CreateProductCommand(string Name,
                                       List<string> Category,
                                       string? Description,
                                       decimal Price,
                                       int StockQuantity) : ICommand<CreateProductResult>;

    public record CreateProductResult(Guid Id);

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Category is required.");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Description must not exceed 2000 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("StockQuantity must not be negative.");
        }
    }

    internal sealed class CreateProductCommandHandler(ICatalogDbContext catalogDbContext) : ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            if (await catalogDbContext.Products.AnyAsync(p => p.Name == ProductName.Of(command.Name) && p.IsActive && !p.IsDeleted, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"A product named '{command.Name}' already exists.");

            var product = Product.Create(ProductName.Of(command.Name),
                                         command.Price,
                                         command.StockQuantity,
                                         command.Category,
                                         command.Description);

            catalogDbContext.Products.Add(product);

            await catalogDbContext.SaveChangesAsync(cancellationToken);

            return new CreateProductResult(product.Id.Value);
        }
    }

    public record CreateProductRequest(string Name,
                                   List<string> Category,
                                   string? Description,
                                   decimal Price,
                                   int StockQuantity);

    public record CreateProductResponse(Guid Id);

    public class CreateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/products", async (CreateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CreateProductCommand(request.Name,
                                                       request.Category,
                                                       request.Description,
                                                       request.Price,
                                                       request.StockQuantity);

                var result = await sender.Send(command, cancellationToken);

                var response = new CreateProductResponse(result.Id);

                return Results.Created($"/api/products/{response.Id}", response);
            })
            .WithName("CreateProduct")
            .WithTags("Products")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
            //.RequireAuthorization();
        }
    }
}