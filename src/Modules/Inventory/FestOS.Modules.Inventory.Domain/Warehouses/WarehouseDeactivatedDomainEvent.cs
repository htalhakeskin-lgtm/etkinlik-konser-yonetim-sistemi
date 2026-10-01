using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.Modules.Inventory.Domain.Warehouses;

/// <summary>A <see cref="Warehouse"/> was deactivated.</summary>
public sealed record WarehouseDeactivatedDomainEvent(WarehouseId WarehouseId) : IDomainEvent;
