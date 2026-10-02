using FestOS.BuildingBlocks.Contracts;

namespace FestOS.Modules.Venues.IntegrationEvents;

/// <summary>
/// Tells other modules that a venue's equipment changed, with the days it touches (venues §7); ordered per
/// venue. Planning marks the overlapping events' requirements stale (1.6).
/// </summary>
public sealed record VenueEquipmentChangedIntegrationEvent : IntegrationEvent
{
    /// <summary>The venue.</summary>
    public required Guid VenueId { get; init; }

    /// <summary>The first day touched, or <see langword="null"/> when open towards the past.</summary>
    public DateOnly? AffectedStart { get; init; }

    /// <summary>The day after the last day touched, or <see langword="null"/> when open towards the future.</summary>
    public DateOnly? AffectedEnd { get; init; }
}
