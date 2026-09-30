using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// An integration event waiting to be delivered, in the <c>outbox_messages</c> table of the module that
/// raised it (database §15). A technical record, so it stays out of the change history.
/// </summary>
[NotAudited]
public sealed class OutboxMessage
{
    /// <summary>The event's message identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The delivery order; an identity column, since UUIDv7 is not ordered within a millisecond.</summary>
    public long Sequence { get; init; }

    /// <summary>The event type, as <c>{full name}, {assembly}</c>.</summary>
    public required string Type { get; init; }

    /// <summary>The record whose events are delivered in order.</summary>
    public Guid OrderingKey { get; init; }

    /// <summary>The event as JSON.</summary>
    public required string Payload { get; init; }

    /// <summary>When the event happened.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>The W3C trace context of the request that raised the event.</summary>
    public string? TraceParent { get; init; }

    /// <summary>When every listener handled it; empty while pending.</summary>
    public DateTimeOffset? DispatchedAt { get; set; }

    /// <summary>How many deliveries failed.</summary>
    public int AttemptCount { get; set; }

    /// <summary>When the next delivery may be tried.</summary>
    public DateTimeOffset? NextAttemptAt { get; set; }

    /// <summary>The last delivery error, shortened.</summary>
    public string? LastError { get; set; }

    /// <summary>Set when the attempts ran out; the event then waits for a manual retry (building-blocks BB-05).</summary>
    public DateTimeOffset? FailedAt { get; set; }
}
