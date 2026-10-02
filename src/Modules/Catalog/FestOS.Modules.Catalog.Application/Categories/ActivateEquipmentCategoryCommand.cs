using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>Opens a deactivated category again, under an active parent (BR-EQP-002).</summary>
public sealed record ActivateEquipmentCategoryCommand(EquipmentCategoryId Id) : ICommand<bool>;
