using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Text;

namespace FestOS.Modules.Catalog.Domain.Categories;

/// <summary>
/// A node of the equipment category tree, e.g. Ses › Mikrofon › Dinamik vokal (US-EQP-001). Riders and venue
/// equipment ask for a category, so it is never deleted, only deactivated (BR-SYS-001). The tree's rules
/// span several categories, so the commands check them (BR-EQP-002).
/// </summary>
public sealed class EquipmentCategory : AggregateRoot<EquipmentCategoryId>, IDeactivatable
{
    /// <summary>The longest name.</summary>
    public const int NameMaxLength = 200;

    private EquipmentCategory(EquipmentCategoryId id)
        : base(id) { }

    /// <summary>The name, unique under the same parent (BR-EQP-011).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The name's search key (database §13): what makes two names the same.</summary>
    [NotAudited]
    public string NameSearch { get; private set; } = string.Empty;

    /// <summary>The parent category; empty for a top-level one.</summary>
    public EquipmentCategoryId? ParentId { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>A new, active category under <paramref name="parentId"/>, or at the top.</summary>
    public static EquipmentCategory Create(string name, EquipmentCategoryId? parentId)
    {
        var category = new EquipmentCategory(EquipmentCategoryId.New());
        category.Edit(name, parentId);
        return category;
    }

    /// <summary>Renames the category and moves it under another parent; the command checks the tree first.</summary>
    public void Edit(string name, EquipmentCategoryId? parentId)
    {
        Name = name.Trim();
        NameSearch = SearchKey.Of(name);
        ParentId = parentId;
    }

    /// <summary>Takes the category out of new selections (BR-SYS-001).</summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
        }
    }

    /// <summary>Opens a deactivated category again.</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }
}
