using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>
/// A model goes only into an existing, active category (BR-EQP-002). The check takes the tree's lock, so the
/// category cannot be deactivated meanwhile (catalog CT-03).
/// </summary>
internal static class ModelCategoryCheck
{
    public static async Task EnsureActiveCategoryAsync(
        this IEquipmentCategoryRepository categories,
        EquipmentCategoryId categoryId,
        CancellationToken cancellationToken
    )
    {
        var tree = new CategoryTree(await categories.LockTreeAsync(cancellationToken));
        CategoryNode node =
            tree.Find(categoryId)
            ?? throw new ValidationFailedException([
                new ValidationError(
                    "CategoryId",
                    "invalidValue",
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                ),
            ]);
        if (!node.IsActive)
        {
            throw CategoryRules.Violation("An inactive category holds no active model.", "inactiveParent");
        }
    }
}
