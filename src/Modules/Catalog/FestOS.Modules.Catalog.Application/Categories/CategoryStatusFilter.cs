namespace FestOS.Modules.Catalog.Application.Categories;

/// <summary>Which categories the tree shows by their status.</summary>
public enum CategoryStatusFilter
{
    /// <summary>Active categories only; the default.</summary>
    Active,

    /// <summary>Deactivated categories only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
