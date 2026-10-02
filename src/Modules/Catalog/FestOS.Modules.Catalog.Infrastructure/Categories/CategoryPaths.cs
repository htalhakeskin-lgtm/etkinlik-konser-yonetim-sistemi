using FestOS.Modules.Catalog.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Categories;

/// <summary>Each category's path from the top, read from the whole tree, which is small (catalog CT-02).</summary>
internal sealed class CategoryPaths
{
    private readonly Dictionary<EquipmentCategoryId, Node> _nodes;

    private CategoryPaths(Dictionary<EquipmentCategoryId, Node> nodes) => _nodes = nodes;

    public static async Task<CategoryPaths> ReadAsync(CatalogDbContext context, CancellationToken cancellationToken)
    {
        List<Node> nodes = await context
            .Categories.AsNoTracking()
            .Select(category => new Node(category.Id, category.Name, category.ParentId, category.DeactivatedAt == null))
            .ToListAsync(cancellationToken);
        return new CategoryPaths(nodes.ToDictionary(node => node.Id));
    }

    public IReadOnlyList<string> PathOf(EquipmentCategoryId id)
    {
        List<string> path = [];
        for (
            EquipmentCategoryId? current = id;
            current is { } step && _nodes.TryGetValue(step, out Node? node);
            current = node.ParentId
        )
        {
            path.Insert(0, node.Name);
        }

        return path;
    }

    public bool Contains(EquipmentCategoryId id) => _nodes.ContainsKey(id);

    public bool IsActive(EquipmentCategoryId id) => _nodes.TryGetValue(id, out Node? node) && node.IsActive;

    public string NameOf(EquipmentCategoryId id) => _nodes[id].Name;

    /// <summary>The category and every category below it (BR-EQP-002: a rule on a category covers its subtree).</summary>
    public IReadOnlyList<EquipmentCategoryId> SelfAndBelow(EquipmentCategoryId id)
    {
        HashSet<EquipmentCategoryId> found = [id];
        bool isGrowing = true;
        while (isGrowing)
        {
            isGrowing = false;
            foreach (Node node in _nodes.Values)
            {
                if (node.ParentId is { } parent && found.Contains(parent) && found.Add(node.Id))
                {
                    isGrowing = true;
                }
            }
        }

        return [.. found];
    }

    private sealed record Node(EquipmentCategoryId Id, string Name, EquipmentCategoryId? ParentId, bool IsActive);
}
