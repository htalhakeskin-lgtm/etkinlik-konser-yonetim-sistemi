using Microsoft.EntityFrameworkCore;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Maps the outbox and inbox tables in every module's schema (database §15).</summary>
internal static class MessagingModel
{
    public const int TypeMaxLength = 512;

    public const int ErrorMaxLength = 2000;

    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(message =>
        {
            message.ToTable("outbox_messages");
            message.Property(outbox => outbox.Sequence).UseIdentityAlwaysColumn();
            message.Property(outbox => outbox.Type).HasMaxLength(TypeMaxLength);
            message.Property(outbox => outbox.Payload).HasColumnType("jsonb");
            message.Property(outbox => outbox.TraceParent).HasMaxLength(64);
            message.Property(outbox => outbox.LastError).HasMaxLength(ErrorMaxLength);

            // The dispatcher reads only what is still waiting, in order.
            message.HasIndex(outbox => outbox.Sequence).HasFilter("dispatched_at IS NULL AND failed_at IS NULL");

            // The ordering check looks for an older undelivered message with the same key.
            message.HasIndex(outbox => new { outbox.OrderingKey, outbox.Sequence }).HasFilter("dispatched_at IS NULL");
        });

        modelBuilder.Entity<InboxMessage>(message =>
        {
            message.ToTable("inbox_messages");
            message.HasKey(inbox => new { inbox.MessageId, inbox.Handler });
            message.Property(inbox => inbox.Handler).HasMaxLength(TypeMaxLength);
        });
    }
}
