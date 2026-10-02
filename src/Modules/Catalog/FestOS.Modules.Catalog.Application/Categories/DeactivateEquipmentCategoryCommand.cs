using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>Deactivates a category with nothing active under it (BR-EQP-002, catalog CT-06).</summary>
public sealed record DeactivateEquipmentCategoryCommand(EquipmentCategoryId Id) : ICommand<bool>;
