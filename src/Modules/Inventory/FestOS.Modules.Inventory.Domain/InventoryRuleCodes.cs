namespace FestOS.Modules.Inventory.Domain;

/// <summary>The rule numbers Inventory reports (03 §5.1, naming §4.2).</summary>
public static class InventoryRuleCodes
{
    /// <summary>The system keeps at least one active warehouse.</summary>
    public const string LastActiveWarehouse = "BR-SYS-013";

    /// <summary>A warehouse name belongs to one warehouse, active or not, whatever the letter case.</summary>
    public const string UniqueWarehouseName = "BR-SYS-016";
}
