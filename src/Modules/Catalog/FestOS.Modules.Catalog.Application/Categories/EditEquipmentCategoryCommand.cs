using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>Renames a category and moves it under another parent (US-EQP-001, BR-EQP-002).</summary>
public sealed record EditEquipmentCategoryCommand(EquipmentCategoryId Id, string Name, EquipmentCategoryId? ParentId)
    : ICommand<bool>;
