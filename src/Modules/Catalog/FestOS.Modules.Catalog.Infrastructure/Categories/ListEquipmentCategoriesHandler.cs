using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Categories;

internal sealed class ListEquipmentCategoriesHandler(CatalogDbContext context)
    : IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>>
{
    public async Task<IReadOnlyList<EquipmentCategoryItem>> HandleAsync(
        ListEquipmentCategoriesQuery query,
        CancellationToken cancellationToken
    )
    {
        // The whole tree is read for the paths; it holds hundreds of categories (catalog CT-02).
        var all = await context
            .Categories.AsNoTracking()
            .OrderBy(category => EF.Functions.Collate(category.Name, Collations.Turkish))
            .ThenBy(category => category.Id)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.ParentId,
                IsActive = category.DeactivatedAt == null,
                category.Version,
            })
            .ToListAsync(cancellationToken);
        var byId = all.ToDictionary(category => category.Id);

        List<string> PathOf(EquipmentCategoryId id)
        {
            List<string> path = [];
            for (
                EquipmentCategoryId? current = id;
                current is { } step && byId.TryGetValue(step, out var node);
                current = node.ParentId
            )
            {
                path.Insert(0, node.Name);
            }

            return path;
        }

        return
        [
            .. all.Where(category =>
                    query.Status switch
                    {
                        CategoryStatusFilter.Active => category.IsActive,
                        CategoryStatusFilter.Inactive => !category.IsActive,
                        _ => true,
                    }
                )
                .Select(category => new EquipmentCategoryItem(
                    category.Id,
                    category.Name,
                    category.ParentId,
                    PathOf(category.Id),
                    ActiveModelCount: 0,
                    category.IsActive,
                    category.Version
                )),
        ];
    }
}
