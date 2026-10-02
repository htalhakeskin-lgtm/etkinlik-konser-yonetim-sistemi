using FestOS.BuildingBlocks.Domain.Events;
using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>A rider got a new version (riders §7).</summary>
public sealed record RiderVersionCreatedDomainEvent(
    RiderId RiderId,
    RiderVersionId RiderVersionId,
    int Number,
    ProductionId? ProductionId
) : IDomainEvent;
