using FestOS.BuildingBlocks.Contracts;

namespace FestOS.Modules.Inventory.IntegrationEvents;

/// <summary>
/// Tells other modules that a warehouse was deactivated and can no longer be chosen (inventory §6);
/// ordered per warehouse. Identity removes it from its warehouse managers.
/// </summary>
public sealed record WarehouseDeactivatedIntegrationEvent : IntegrationEvent
{
    /// <summary>The warehouse.</summary>
    public required Guid WarehouseId { get; init; }
}
