using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Defines a new warehouse (US-SYS-005).</summary>
public sealed record CreateWarehouseCommand(string Name, string City, string Address) : ICommand<WarehouseId>;
