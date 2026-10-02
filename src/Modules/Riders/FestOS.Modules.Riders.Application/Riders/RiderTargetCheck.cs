using System.Globalization;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>
/// Every model and category of a new version exists and is active, asked of Catalog (BR-RDR-001,
/// BR-SYS-001); a line brought over from an older version with a target deactivated since must be changed
/// first (riders RD-04).
/// </summary>
internal static class RiderTargetCheck
{
    public static async Task EnsureTargetsActiveAsync(
        this ICatalogDirectory catalog,
        IReadOnlyList<RiderLineDetails> lines,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyDictionary<Guid, ModelSummary> models = await catalog.FindModelsAsync(
            [.. lines.SelectMany(line => line.EquivalentModelIds.Prepend(line.ModelId ?? Guid.Empty)).Distinct()],
            cancellationToken
        );
        IReadOnlyDictionary<Guid, CategorySummary> categories = await catalog.FindCategoriesAsync(
            [.. lines.Select(line => line.CategoryId).OfType<Guid>().Distinct()],
            cancellationToken
        );

        List<ValidationError> errors = [];
        for (int index = 0; index < lines.Count; index++)
        {
            RiderLineDetails line = lines[index];
            if (line.ModelId is { } modelId && !IsActive(models, modelId))
            {
                errors.Add(Invalid(Field(index, "ModelId")));
            }

            if (
                line.CategoryId is { } categoryId
                && !(categories.TryGetValue(categoryId, out CategorySummary? category) && category.IsActive)
            )
            {
                errors.Add(Invalid(Field(index, "CategoryId")));
            }

            for (int position = 0; position < line.EquivalentModelIds.Count; position++)
            {
                if (!IsActive(models, line.EquivalentModelIds[position]))
                {
                    errors.Add(
                        Invalid(Field(index, $"EquivalentModelIds[{position.ToString(CultureInfo.InvariantCulture)}]"))
                    );
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationFailedException(errors);
        }
    }

    private static bool IsActive(IReadOnlyDictionary<Guid, ModelSummary> models, Guid id) =>
        models.TryGetValue(id, out ModelSummary? model) && model.IsActive;

    private static string Field(int index, string name) =>
        string.Create(CultureInfo.InvariantCulture, $"Lines[{index}].{name}");

    private static ValidationError Invalid(string field) =>
        new(field, "invalidValue", new Dictionary<string, object?>(StringComparer.Ordinal));
}
