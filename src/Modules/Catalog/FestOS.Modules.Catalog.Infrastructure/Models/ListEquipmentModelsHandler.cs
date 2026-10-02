using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Catalog.Application.Models;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;
using FestOS.Modules.Catalog.Infrastructure.Categories;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Models;

internal sealed class ListEquipmentModelsHandler(CatalogDbContext context)
    : IQueryHandler<ListEquipmentModelsQuery, PagedResult<EquipmentModelListItem>>
{
    public static SortKeys<EquipmentModel> SortKeys { get; } =
        new SortKeys<EquipmentModel>()
            .Add("name", model => EF.Functions.Collate(model.Brand + " " + model.Name, Collations.Turkish))
            .Add("brand", model => EF.Functions.Collate(model.Brand, Collations.Turkish));

    public async Task<PagedResult<EquipmentModelListItem>> HandleAsync(
        ListEquipmentModelsQuery query,
        CancellationToken cancellationToken
    )
    {
        CategoryPaths paths = await CategoryPaths.ReadAsync(context, cancellationToken);
        IQueryable<EquipmentModel> models = context.Models.AsNoTracking();
        models = query.Status switch
        {
            ModelStatusFilter.Active => models.Where(model => model.DeactivatedAt == null),
            ModelStatusFilter.Inactive => models.Where(model => model.DeactivatedAt != null),
            _ => models,
        };
        if (query.CategoryId is { } categoryId)
        {
            List<EquipmentCategoryId> subtree = [.. paths.SelfAndBelow(categoryId)];
            models = models.Where(model => subtree.Contains(model.CategoryId));
        }

        if (query.TrackingType is { } trackingType)
        {
            models = models.Where(model => model.TrackingType == trackingType);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string key = SearchKey.Of(query.Q);
            models = models.Where(model => model.BrandNameSearch.Contains(key));
        }

        PagedResult<EquipmentModelListItem> page = await SortKeys
            .Apply(models, SortSpec.Parse(query.Sort, ListEquipmentModelsQuery.DefaultSort), model => model.Id)
            .Select(model => new EquipmentModelListItem(
                model.Id,
                model.Brand,
                model.Name,
                model.CategoryId,
                Array.Empty<string>(),
                model.TrackingType,
                model.WeightKilograms,
                model.PowerWatts,
                model.DeactivatedAt == null,
                model.Version
            ))
            .ToPagedResultAsync(query.Page, cancellationToken);
        return page with
        {
            Items = [.. page.Items.Select(item => item with { CategoryPath = paths.PathOf(item.CategoryId) })],
        };
    }
}
