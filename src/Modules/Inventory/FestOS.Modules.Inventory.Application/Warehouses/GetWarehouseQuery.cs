using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>One warehouse, for the edit dialog.</summary>
public sealed record GetWarehouseQuery(WarehouseId Id) : IQuery<WarehouseDetails>;
