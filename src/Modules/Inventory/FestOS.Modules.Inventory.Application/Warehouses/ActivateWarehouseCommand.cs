using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Opens a deactivated warehouse again (IN-03).</summary>
public sealed record ActivateWarehouseCommand(WarehouseId Id) : ICommand<bool>;
