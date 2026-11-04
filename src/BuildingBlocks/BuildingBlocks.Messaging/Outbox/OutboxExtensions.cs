using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Outbox
{
    public static class OutboxExtensions
    {
        public static ModelBuilder ApplyOutboxConfiguration(
            this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

            return modelBuilder;
        }
    }
}