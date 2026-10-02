using FestOS.BuildingBlocks.Contracts;

namespace FestOS.Modules.Riders.IntegrationEvents;

/// <summary>
/// Tells other modules that a rider has a new version (riders §7); ordered per rider. Booking marks the
/// events that use an older version of a production's rider (1.4).
/// </summary>
public sealed record RiderVersionCreatedIntegrationEvent : IntegrationEvent
{
    /// <summary>The rider.</summary>
    public required Guid RiderId { get; init; }

    /// <summary>The new version.</summary>
    public required Guid RiderVersionId { get; init; }

    /// <summary>The new version's number.</summary>
    public required int Number { get; init; }

    /// <summary>The production the rider belongs to, when it comes from one.</summary>
    public Guid? ProductionId { get; init; }
}
