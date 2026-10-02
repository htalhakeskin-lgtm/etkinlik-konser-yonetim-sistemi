using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>
/// A line's model or category exists and is active, asked of Catalog (BR-VEN-001, BR-SYS-001); a line that
/// keeps its target keeps it even if the target was deactivated since.
/// </summary>
internal static class EquipmentTargetCheck
{
    public static async Task EnsureTargetFitsAsync(
        this ICatalogDirectory catalog,
        VenueEquipmentDetails details,
        VenueEquipment? kept,
        CancellationToken cancellationToken
    )
    {
        if (details.ModelId is { } modelId && kept?.ModelId != modelId)
        {
            IReadOnlyDictionary<Guid, ModelSummary> models = await catalog.FindModelsAsync(
                [modelId],
                cancellationToken
            );
            if (!models.TryGetValue(modelId, out ModelSummary? model) || !model.IsActive)
            {
                throw Invalid("ModelId");
            }
        }

        if (details.CategoryId is { } categoryId && kept?.CategoryId != categoryId)
        {
            IReadOnlyDictionary<Guid, CategorySummary> categories = await catalog.FindCategoriesAsync(
                [categoryId],
                cancellationToken
            );
            if (!categories.TryGetValue(categoryId, out CategorySummary? category) || !category.IsActive)
            {
                throw Invalid("CategoryId");
            }
        }
    }

    private static ValidationFailedException Invalid(string field) =>
        new([new ValidationError(field, "invalidValue", new Dictionary<string, object?>(StringComparer.Ordinal))]);
}
