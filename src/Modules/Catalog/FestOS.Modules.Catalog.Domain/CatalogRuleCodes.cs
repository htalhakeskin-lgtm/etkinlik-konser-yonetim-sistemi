namespace FestOS.Modules.Catalog.Domain;

/// <summary>The rule numbers Catalog reports (03 §5.6, naming §4.2).</summary>
public static class CatalogRuleCodes
{
    /// <summary>The category tree has no cycle, and an inactive category holds nothing active.</summary>
    public const string CategoryHierarchy = "BR-EQP-002";

    /// <summary>A category name belongs to one category under the same parent, active or not.</summary>
    public const string UniqueCategoryName = "BR-EQP-011";
}
