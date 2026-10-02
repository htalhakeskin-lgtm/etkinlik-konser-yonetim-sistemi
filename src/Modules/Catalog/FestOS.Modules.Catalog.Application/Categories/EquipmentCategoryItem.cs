using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>A category of the tree; <c>Version</c> lets its actions send <c>If-Match</c> (api §9).</summary>
/// <param name="Id">The category.</param>
/// <param name="Name">Its name.</param>
/// <param name="ParentId">Its parent, empty at the top.</param>
/// <param name="Path">The names from the top down to it, e.g. Ses, Mikrofon, Dinamik vokal.</param>
/// <param name="ActiveModelCount">How many active models it holds directly.</param>
/// <param name="IsActive">Whether it can be chosen for new work.</param>
/// <param name="Version">The version to send back.</param>
public sealed record EquipmentCategoryItem(
    EquipmentCategoryId Id,
    string Name,
    EquipmentCategoryId? ParentId,
    IReadOnlyList<string> Path,
    int ActiveModelCount,
    bool IsActive,
    int Version
);
