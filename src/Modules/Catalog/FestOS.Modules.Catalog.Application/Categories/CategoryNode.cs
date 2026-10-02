using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>A category's place in the tree, for the commands' checks (BR-EQP-002).</summary>
public sealed record CategoryNode(EquipmentCategoryId Id, EquipmentCategoryId? ParentId, bool IsActive);
