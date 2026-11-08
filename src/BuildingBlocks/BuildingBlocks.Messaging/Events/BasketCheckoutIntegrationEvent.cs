using BuildingBlocks.Common.Dtos;

namespace BuildingBlocks.Messaging.Events
{
    public record BasketCheckoutIntegrationEvent(
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
        List<OrderItemDto> OrderItems);
}
