namespace FestOS.BuildingBlocks.Contracts;

/// <summary>
/// The base of every integration event. Events are immutable records with init-only members and names
/// ending in <c>IntegrationEvent</c> (AT-06).
/// </summary>
/// <example>
/// <code>
/// public sealed record EventConfirmedIntegrationEvent : IntegrationEvent
/// {
///     public required Guid EventId { get; init; }
/// }
///
/// outbox.Add(new EventConfirmedIntegrationEvent { EventId = id, OrderingKey = id, OccurredAt = now });
/// </code>
/// </example>
public abstract record IntegrationEvent : IIntegrationEvent
{
    /// <inheritdoc />
    public Guid MessageId { get; init; } = Guid.CreateVersion7();

    /// <inheritdoc />
    public required DateTimeOffset OccurredAt { get; init; }

    /// <inheritdoc />
    public required Guid OrderingKey { get; init; }
}
