using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>Loads and adds categories for commands (identity ID-01); the unit of work saves them.</summary>
public interface IEquipmentCategoryRepository
{
    /// <summary>The category, or <see langword="null"/>.</summary>
    Task<EquipmentCategory?> FindAsync(EquipmentCategoryId id, CancellationToken cancellationToken);

    /// <summary>
    /// Takes the tree's lock and reads every category's place, so two moves at once cannot build a cycle and a
    /// deactivation sees the children added meanwhile (catalog CT-03).
    /// </summary>
    Task<IReadOnlyList<CategoryNode>> LockTreeAsync(CancellationToken cancellationToken);

    /// <summary>Adds a new category.</summary>
    void Add(EquipmentCategory category);
}
