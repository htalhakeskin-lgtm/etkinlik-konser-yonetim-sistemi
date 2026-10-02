using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>A model in a kit's contents opened down to models, with the total quantity (BR-EQP-003).</summary>
public sealed record KitContentItem(EquipmentModelId ModelId, string Name, int Quantity);
