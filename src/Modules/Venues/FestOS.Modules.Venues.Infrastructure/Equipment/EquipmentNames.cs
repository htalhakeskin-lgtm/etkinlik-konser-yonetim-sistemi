using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

/// <summary>The names of the models and categories a venue's lines ask for, from Catalog in two calls (parties MD-02).</summary>
internal sealed class EquipmentNames
{
    private readonly IReadOnlyDictionary<Guid, ModelSummary> _models;
    private readonly IReadOnlyDictionary<Guid, CategorySummary> _categories;

    private EquipmentNames(
        IReadOnlyDictionary<Guid, ModelSummary> models,
        IReadOnlyDictionary<Guid, CategorySummary> categories
    )
    {
        _models = models;
        _categories = categories;
    }

    public static async Task<EquipmentNames> ReadAsync(
        ICatalogDirectory catalog,
        IReadOnlyCollection<VenueEquipment> lines,
        CancellationToken cancellationToken
    ) =>
        new(
            await catalog.FindModelsAsync([.. lines.Select(line => line.ModelId).OfType<Guid>()], cancellationToken),
            await catalog.FindCategoriesAsync(
                [.. lines.Select(line => line.CategoryId).OfType<Guid>()],
                cancellationToken
            )
        );

    public (string Name, IReadOnlyList<string> Path, bool IsActive) Of(VenueEquipment line)
    {
        if (line.ModelId is { } modelId && _models.TryGetValue(modelId, out ModelSummary? model))
        {
            return (model.DisplayName, model.CategoryPath, model.IsActive);
        }

        if (line.CategoryId is { } categoryId && _categories.TryGetValue(categoryId, out CategorySummary? category))
        {
            return (string.Join(" › ", category.Path), category.Path, category.IsActive);
        }

        return (line.Description ?? string.Empty, [], true);
    }
}
