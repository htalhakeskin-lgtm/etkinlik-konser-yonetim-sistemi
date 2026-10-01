using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Changes a warehouse's name, city and address (US-SYS-005).</summary>
public sealed record EditWarehouseCommand(WarehouseId Id, string Name, string City, string Address) : ICommand<bool>;
