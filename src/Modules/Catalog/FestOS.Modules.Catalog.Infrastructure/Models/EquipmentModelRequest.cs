using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Infrastructure.Models;

/// <summary>
/// The body of <c>POST /api/v1/equipment-models</c> and <c>PUT /api/v1/equipment-models/{modelId}</c>; the
/// technical values may be left out.
/// </summary>
public sealed record EquipmentModelRequest(
    string Brand,
    string Name,
    Guid CategoryId,
    TrackingType TrackingType,
    decimal? WeightKilograms = null,
    int? PowerWatts = null,
    decimal? TransportVolumeCubicMeters = null
)
{
    /// <summary>The technical values as the model takes them.</summary>
    public ModelMeasures Measures() => new(WeightKilograms, PowerWatts, TransportVolumeCubicMeters);
}
