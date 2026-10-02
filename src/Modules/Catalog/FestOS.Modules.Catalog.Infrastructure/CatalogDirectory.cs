using System.Text.Json;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;
using FestOS.Modules.Catalog.Infrastructure.Categories;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure;

/// <summary>Answers other modules from the Catalog schema, with its own role (05 §2, principle 1).</summary>
internal sealed class CatalogDirectory(CatalogDbContext context) : ICatalogDirectory
{
    public async Task<IReadOnlyDictionary<Guid, ModelSummary>> FindModelsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        // EF compares the typed identifiers, not their Value (inventory §6).
        List<EquipmentModelId> modelIds = [.. ids.Distinct().Select(EquipmentModelId.From)];
        var models = await context
            .Models.AsNoTracking()
            .Where(model => modelIds.Contains(model.Id))
            .Select(model => new
            {
                model.Id,
                model.Brand,
                model.Name,
                model.CategoryId,
                model.TrackingType,
                IsActive = model.DeactivatedAt == null,
            })
            .ToListAsync(cancellationToken);
        CategoryPaths paths = await CategoryPaths.ReadAsync(context, cancellationToken);
        return models.ToDictionary(
            model => model.Id.Value,
            model => new ModelSummary(
                model.Id.Value,
                $"{model.Brand} {model.Name}",
                model.CategoryId.Value,
                paths.PathOf(model.CategoryId),
                JsonNamingPolicy.CamelCase.ConvertName(model.TrackingType.ToString()),
                model.IsActive
            )
        );
    }

    public async Task<IReadOnlyDictionary<Guid, KitSummary>> FindKitsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        List<KitId> kitIds = [.. ids.Distinct().Select(KitId.From)];
        return await context
            .Kits.AsNoTracking()
            .Where(kit => kitIds.Contains(kit.Id))
            .ToDictionaryAsync(
                kit => kit.Id.Value,
                kit => new KitSummary(kit.Id.Value, kit.Name, kit.DeactivatedAt == null),
                cancellationToken
            );
    }

    public async Task<IReadOnlyDictionary<Guid, CategorySummary>> FindCategoriesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        CategoryPaths paths = await CategoryPaths.ReadAsync(context, cancellationToken);
        return ids.Distinct()
            .Select(EquipmentCategoryId.From)
            .Where(paths.Contains)
            .ToDictionary(
                id => id.Value,
                id => new CategorySummary(id.Value, paths.NameOf(id), paths.PathOf(id), paths.IsActive(id))
            );
    }
}
