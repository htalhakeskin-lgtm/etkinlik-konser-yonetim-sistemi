namespace FestOS.Modules.Inventory.Contracts;

/// <summary>
/// The permissions Inventory defines (inventory §5, naming §8.1). They live in the contracts, since the
/// role matrix in Identity grants them (identity §5.2).
/// </summary>
public static class InventoryPermissions
{
    /// <summary>Lists and opens warehouses.</summary>
    public const string ViewWarehouses = "Inventory.Warehouses.View";

    /// <summary>Creates warehouses.</summary>
    public const string CreateWarehouses = "Inventory.Warehouses.Create";

    /// <summary>Changes a warehouse's name, city and address.</summary>
    public const string EditWarehouses = "Inventory.Warehouses.Edit";

    /// <summary>Deactivates and reactivates warehouses.</summary>
    public const string DeactivateWarehouses = "Inventory.Warehouses.Deactivate";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } =
    [ViewWarehouses, CreateWarehouses, EditWarehouses, DeactivateWarehouses];
}
