namespace FestOS.Modules.Catalog.Infrastructure.Categories;

/// <summary>
/// The body of <c>POST /api/v1/equipment-categories</c> and <c>PUT /api/v1/equipment-categories/{categoryId}</c>;
/// a top-level category has no parent.
/// </summary>
public sealed record EquipmentCategoryRequest(string Name, Guid? ParentId = null);
