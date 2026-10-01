namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Which warehouses a list shows by their status.</summary>
public enum WarehouseStatusFilter
{
    /// <summary>Active warehouses only; the default.</summary>
    Active,

    /// <summary>Deactivated warehouses only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
