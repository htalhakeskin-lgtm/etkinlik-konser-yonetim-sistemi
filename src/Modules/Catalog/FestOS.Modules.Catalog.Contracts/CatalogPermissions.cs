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

    /// <summary>Lists and opens models.</summary>
    public const string ViewModels = "Catalog.Models.View";

    /// <summary>Creates models.</summary>
    public const string CreateModels = "Catalog.Models.Create";

    /// <summary>Changes models.</summary>
    public const string EditModels = "Catalog.Models.Edit";

    /// <summary>Deactivates and reactivates models.</summary>
    public const string DeactivateModels = "Catalog.Models.Deactivate";

    /// <summary>Lists and opens kits.</summary>
    public const string ViewKits = "Catalog.Kits.View";

    /// <summary>Creates kits.</summary>
    public const string CreateKits = "Catalog.Kits.Create";

    /// <summary>Changes a kit's name and lines.</summary>
    public const string EditKits = "Catalog.Kits.Edit";

    /// <summary>Deactivates and reactivates kits.</summary>
    public const string DeactivateKits = "Catalog.Kits.Deactivate";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } =
    [
        ViewCategories,
        CreateCategories,
        EditCategories,
        DeactivateCategories,
        ViewModels,
        CreateModels,
        EditModels,
        DeactivateModels,
        ViewKits,
        CreateKits,
        EditKits,
        DeactivateKits,
    ];
}
