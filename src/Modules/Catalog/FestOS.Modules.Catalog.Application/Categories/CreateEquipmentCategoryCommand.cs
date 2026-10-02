using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>Adds a category to the tree (US-EQP-001).</summary>
public sealed record CreateEquipmentCategoryCommand(string Name, EquipmentCategoryId? ParentId)
    : ICommand<EquipmentCategoryId>;
