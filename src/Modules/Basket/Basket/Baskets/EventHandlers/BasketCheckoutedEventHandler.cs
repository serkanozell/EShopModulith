using Basket.Baskets.Events;
using BuildingBlocks.Messaging.Events;
using Microsoft.Extensions.Logging;

namespace Basket.Baskets.EventHandlers
{
    public sealed class BasketCheckoutedEventHandler(BasketDbContext basketDbContext, ILogger<BasketCheckoutedEventHandler> logger) : INotificationHandler<BasketCheckoutedEvent>
    {
        public Task Handle(BasketCheckoutedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Domain event handled: {DomainEvent} - BasketId: {BasketId}, UserName: {UserName}",
                                  notification.GetType().Name,
                                  notification.BasketId,
                                  notification.UserName);

            var basketCheckoutIntegrationEvent = new BasketCheckoutIntegrationEvent
            (
                notification.UserName,
                notification.CustomerId,
                notification.TotalPrice,
                notification.FirstName,
                notification.LastName,
                notification.EmailAddress,
                notification.AddressLine,
                notification.Country,
                notification.State,
                notification.ZipCode,
                notification.CardName,
                notification.CardNumber,
                notification.Expiration,
                notification.Cvv,
                notification.PaymentMethod,
                notification.OrderItems
            );

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = typeof(BasketCheckoutIntegrationEvent).FullName!,
                Content = JsonSerializer.Serialize(basketCheckoutIntegrationEvent),
                OccurredOn = DateTime.UtcNow
            };

            basketDbContext.OutboxMessages.Add(outboxMessage);

            return Task.CompletedTask;
        }
    }
}