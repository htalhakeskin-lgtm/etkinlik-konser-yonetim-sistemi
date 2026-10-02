namespace FestOS.Modules.Catalog.Domain;

/// <summary>The rule numbers Catalog reports (03 §5.6, naming §4.2).</summary>
public static class CatalogRuleCodes
{
    /// <summary>A model's tracking type stays once it has stock.</summary>
    public const string TrackingTypeFixed = "BR-EQP-001";

    /// <summary>The category tree has no cycle, and an inactive category holds nothing active.</summary>
    public const string CategoryHierarchy = "BR-EQP-002";

    /// <summary>A kit never holds itself, directly or through another kit.</summary>
    public const string KitStructure = "BR-EQP-003";

    /// <summary>A category name belongs to one category under the same parent, active or not.</summary>
    public const string UniqueCategoryName = "BR-EQP-011";

    /// <summary>A brand and model name belong to one model, active or not.</summary>
    public const string UniqueModel = "BR-EQP-012";

    /// <summary>A kit name belongs to one kit, active or not.</summary>
    public const string UniqueKitName = "BR-EQP-013";
}
