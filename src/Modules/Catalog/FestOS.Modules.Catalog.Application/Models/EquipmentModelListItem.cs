using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>A row of the models list; <c>Version</c> lets the row's actions send <c>If-Match</c> (api §9).</summary>
public sealed record EquipmentModelListItem(
    EquipmentModelId Id,
    string Brand,
    string Name,
    EquipmentCategoryId CategoryId,
    IReadOnlyList<string> CategoryPath,
    TrackingType TrackingType,
    decimal? WeightKilograms,
    int? PowerWatts,
    bool IsActive,
    int Version
);
