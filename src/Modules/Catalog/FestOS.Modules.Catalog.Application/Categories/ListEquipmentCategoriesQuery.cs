using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>
/// The category tree as a flat list, in Turkish order with each category's path; the screen builds the tree
/// (catalog CT-02). It is small, so it has no pages.
/// </summary>
public sealed record ListEquipmentCategoriesQuery(CategoryStatusFilter Status)
    : IQuery<IReadOnlyList<EquipmentCategoryItem>>;
