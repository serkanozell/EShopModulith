using BuildingBlocks.Common.DDD;
using BuildingBlocks.Common.Dtos;

namespace Basket.Baskets.Events
{
    public record BasketCheckoutedEvent(
        Guid BasketId,
        string UserName,
        Guid CustomerId,
        decimal TotalPrice,
        // Shipping and BillingAddress
        string FirstName,
        string LastName,
        string EmailAddress,
        string AddressLine,
        string Country,
        string State,
        string ZipCode,
        //Payment
        string CardName,
        string CardNumber,
        string Expiration,
        string Cvv,
        int PaymentMethod,
        List<OrderItemDto> OrderItems) : DomainEvent;
}