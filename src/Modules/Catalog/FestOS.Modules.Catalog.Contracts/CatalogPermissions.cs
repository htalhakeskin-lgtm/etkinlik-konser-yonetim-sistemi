namespace FestOS.Modules.Catalog.Contracts;

/// <summary>The permissions Catalog defines (catalog §6, naming §8.1).</summary>
public static class CatalogPermissions
{
    /// <summary>Lists the category tree.</summary>
    public const string ViewCategories = "Catalog.Categories.View";

    /// <summary>Creates categories.</summary>
    public const string CreateCategories = "Catalog.Categories.Create";

    /// <summary>Renames and moves categories.</summary>
    public const string EditCategories = "Catalog.Categories.Edit";

    /// <summary>Deactivates and reactivates categories.</summary>
    public const string DeactivateCategories = "Catalog.Categories.Deactivate";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } =
    [ViewCategories, CreateCategories, EditCategories, DeactivateCategories];
}
