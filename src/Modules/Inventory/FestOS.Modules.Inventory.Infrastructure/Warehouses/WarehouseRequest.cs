namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

/// <summary>The body of <c>POST /api/v1/warehouses</c> and <c>PUT /api/v1/warehouses/{warehouseId}</c>.</summary>
public sealed record WarehouseRequest(string Name, string City, string Address);
