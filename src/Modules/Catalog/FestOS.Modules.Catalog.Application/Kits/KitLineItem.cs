using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>A line of a kit with the name of its model or kit; the form sends the target back.</summary>
public sealed record KitLineItem(
    KitLineId Id,
    EquipmentModelId? ModelId,
    KitId? SubKitId,
    string Name,
    int Quantity,
    bool IsActive
);
