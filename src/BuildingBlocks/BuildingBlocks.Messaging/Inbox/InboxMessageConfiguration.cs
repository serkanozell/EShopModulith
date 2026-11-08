using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Messaging.Inbox
{
    public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("inbox_messages");

            builder.HasKey(x => x.Id);

            // Id, event'ten gelir; EF üretmemeli.
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.OccurredOn).IsRequired();
            builder.Property(x => x.ReceivedOn).IsRequired();

            builder.HasIndex(x => new { x.ProcessedOn, x.OccurredOn });
        }
    }
}