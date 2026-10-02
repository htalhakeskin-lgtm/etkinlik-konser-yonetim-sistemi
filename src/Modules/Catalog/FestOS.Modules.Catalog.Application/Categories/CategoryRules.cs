using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Catalog.Domain;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>The tree rules the category commands share (BR-EQP-002, catalog CT-06).</summary>
internal static class CategoryRules
{
    /// <summary>A parent must exist and be active; a category cannot move under itself or its own children.</summary>
    public static void EnsureParentFits(
        CategoryTree tree,
        EquipmentCategoryId? parentId,
        EquipmentCategoryId? movedCategory
    )
    {
        if (parentId is not { } parent)
        {
            return;
        }

        CategoryNode node =
            tree.Find(parent)
            ?? throw new ValidationFailedException([
                new ValidationError(
                    "ParentId",
                    "invalidValue",
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                ),
            ]);
        if (!node.IsActive)
        {
            throw Violation("An inactive category holds no active category.", "inactiveParent");
        }

        if (movedCategory is { } moved && tree.IsSelfOrBelow(parent, moved))
        {
            throw Violation("A category cannot move under itself or its own children.", "cycle");
        }
    }

    public static BusinessRuleViolationException Violation(
        string message,
        string reason,
        IReadOnlyDictionary<string, object?>? extra = null
    )
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { ["reason"] = reason };
        foreach ((string key, object? value) in extra ?? new Dictionary<string, object?>(StringComparer.Ordinal))
        {
            parameters[key] = value;
        }

        return new BusinessRuleViolationException(CatalogRuleCodes.CategoryHierarchy, message, parameters: parameters);
    }
}
