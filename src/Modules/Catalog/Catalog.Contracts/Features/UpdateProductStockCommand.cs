using BuildingBlocks.Common.CQRS;

namespace Catalog.Contracts.Features
{
    public record UpdateProductStockCommand(
        Guid ProductId,
        bool IsInStock,
        int AvailableQuantity,
        DateTime OccurredOn) : ICommand<UpdateProductStockCommandResult>;

    public record UpdateProductStockCommandResult(bool IsSuccess);
}