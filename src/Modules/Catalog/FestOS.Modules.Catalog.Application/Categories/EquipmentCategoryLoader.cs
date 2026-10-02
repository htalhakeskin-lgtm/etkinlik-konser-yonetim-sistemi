using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>Loads the category a command changes, at the version the request saw (api §9).</summary>
internal static class EquipmentCategoryLoader
{
    public static async Task<EquipmentCategory> LoadForChangeAsync(
        this IEquipmentCategoryRepository categories,
        EquipmentCategoryId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        EquipmentCategory category =
            await categories.FindAsync(id, cancellationToken)
            ?? throw new NotFoundException("EquipmentCategory", id.Value);
        expectedVersion.EnsureMatches(category);
        return category;
    }
}
