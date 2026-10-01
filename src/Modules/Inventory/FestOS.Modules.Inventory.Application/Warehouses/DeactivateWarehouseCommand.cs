using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Deactivates a warehouse; the last active one stays (BR-SYS-013).</summary>
public sealed record DeactivateWarehouseCommand(WarehouseId Id) : ICommand<bool>;
