using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Messaging.Outbox
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                   .IsRequired();

            builder.Property(x => x.Content)
                   .IsRequired();

            builder.Property(x => x.OccurredOn)
                   .IsRequired();

            builder.HasIndex(x => new
            {
                x.ProcessedOn,
                x.OccurredOn
            });
        }
    }
}