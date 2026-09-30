using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// An integration event a listener already handled, in the <c>inbox_messages</c> table of the listener's
/// module; the same listener never handles the same event twice (database §15).
/// </summary>
[NotAudited]
public sealed class InboxMessage
{
    /// <summary>The event's message identifier.</summary>
    public Guid MessageId { get; init; }

    /// <summary>The listener's type name.</summary>
    public required string Handler { get; init; }

    /// <summary>When the listener handled it.</summary>
    public DateTimeOffset ProcessedAt { get; init; }
}
