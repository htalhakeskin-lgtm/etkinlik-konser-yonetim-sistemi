using FestOS.Modules.Catalog.Application.Kits;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>A model as its page shows it; <see cref="Version"/> is also the <c>ETag</c> (api §9).</summary>
public sealed record EquipmentModelDetails(
    EquipmentModelId Id,
    string Brand,
    string Name,
    EquipmentCategoryId CategoryId,
    IReadOnlyList<string> CategoryPath,
    TrackingType TrackingType,
    decimal? WeightKilograms,
    int? PowerWatts,
    decimal? TransportVolumeCubicMeters,
    bool HasStock,
    IReadOnlyList<KitReference> Kits,
    DateTimeOffset? DeactivatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version
);
