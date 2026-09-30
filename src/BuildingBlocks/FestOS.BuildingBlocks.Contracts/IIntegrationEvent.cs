namespace FestOS.BuildingBlocks.Contracts;

/// <summary>
/// Something that happened in one module that other modules react to (05 §7). Written to the outbox
/// with the change that caused it and delivered at least once (05 §9.2).
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// The message's identifier; listeners record it in their inbox to handle it once. Named apart from
    /// the business event's own <c>EventId</c>, which events about concert events carry.
    /// </summary>
    Guid MessageId { get; }

    /// <summary>When the event happened (UTC).</summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>The record whose events are delivered in order, e.g. the concert event whose status changed.</summary>
    Guid OrderingKey { get; }
}
