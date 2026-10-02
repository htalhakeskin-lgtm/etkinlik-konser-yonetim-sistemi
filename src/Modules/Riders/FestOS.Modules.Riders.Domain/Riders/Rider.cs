using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Domain.Riders;

/// <summary>
/// The technical needs of a production as a series of versions (US-RDR-001). It opens empty with its
/// production (riders RD-01); each save adds the next version, never changing an earlier one.
/// </summary>
public sealed class Rider : AggregateRoot<RiderId>
{
    private Rider(RiderId id)
        : base(id) { }

    /// <summary>Where it comes from.</summary>
    public RiderSource Source { get; private set; }

    /// <summary>The production it belongs to, when it comes from one.</summary>
    public ProductionId? ProductionId { get; private set; }

    /// <summary>The number of its newest version; 0 while it has none.</summary>
    public int LatestVersionNumber { get; private set; }

    /// <summary>The empty rider of a new production (BR-RDR-007).</summary>
    public static Rider ForProduction(ProductionId productionId) =>
        new(RiderId.New()) { Source = RiderSource.Production, ProductionId = productionId };
}
