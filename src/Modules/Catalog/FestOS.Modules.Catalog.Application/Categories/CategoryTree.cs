using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>The category tree as the commands read it under its lock (catalog CT-03).</summary>
internal sealed class CategoryTree(IReadOnlyList<CategoryNode> nodes)
{
    private readonly Dictionary<EquipmentCategoryId, CategoryNode> _byId = nodes.ToDictionary(node => node.Id);

    public CategoryNode? Find(EquipmentCategoryId id) => _byId.GetValueOrDefault(id);

    /// <summary>Whether <paramref name="candidate"/> is <paramref name="category"/> or lies under it.</summary>
    public bool IsSelfOrBelow(EquipmentCategoryId candidate, EquipmentCategoryId category)
    {
        for (EquipmentCategoryId? current = candidate; current is { } id; current = Find(id)?.ParentId)
        {
            if (id == category)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>How many active categories sit directly under <paramref name="category"/>.</summary>
    public int ActiveChildCount(EquipmentCategoryId category) =>
        nodes.Count(node => node.ParentId == category && node.IsActive);
}
