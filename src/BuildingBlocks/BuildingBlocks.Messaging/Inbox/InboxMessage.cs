namespace BuildingBlocks.Messaging.Inbox
{
    public class InboxMessage
    {
        /// <summary>IntegrationEvent.EventId. Primary key olduğu için idempotency anahtarıdır.</summary>
        public Guid Id { get; set; }

        public string Type { get; set; } = default!;

        public string Content { get; set; } = default!;

        /// <summary>Event'in oluştuğu zaman.</summary>
        public DateTime OccurredOn { get; set; }

        /// <summary>Mesajın inbox'a yazıldığı zaman.</summary>
        public DateTime ReceivedOn { get; set; }

        public DateTime? ProcessedOn { get; set; }

        public int RetryCount { get; set; }

        public string? Error { get; set; }
    }
}