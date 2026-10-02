using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Domain.Kits;

/// <summary>A kit line as the form sends it: a model or another kit, and how many (US-EQP-005).</summary>
/// <param name="ModelId">The model, or <see langword="null"/> for a kit line.</param>
/// <param name="SubKitId">The kit, or <see langword="null"/> for a model line.</param>
/// <param name="Quantity">How many, at least one.</param>
public sealed record KitLineDetails(EquipmentModelId? ModelId, KitId? SubKitId, int Quantity);
