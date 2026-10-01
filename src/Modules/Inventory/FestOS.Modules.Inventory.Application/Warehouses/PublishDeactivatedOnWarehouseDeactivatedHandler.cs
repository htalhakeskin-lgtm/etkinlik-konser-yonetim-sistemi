using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Inventory.Domain.Warehouses;
using FestOS.Modules.Inventory.IntegrationEvents;

namespace FestOS.Modules.Inventory.Application.Warehouses;

/// <summary>Turns the domain event into the integration event, written to the outbox by the same save.</summary>
internal sealed class PublishDeactivatedOnWarehouseDeactivatedHandler(IOutbox outbox, TimeProvider timeProvider)
    : IDomainEventHandler<WarehouseDeactivatedDomainEvent>
{
    public Task HandleAsync(WarehouseDeactivatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Add(
            new WarehouseDeactivatedIntegrationEvent
            {
                WarehouseId = domainEvent.WarehouseId.Value,
                OrderingKey = domainEvent.WarehouseId.Value,
                OccurredAt = timeProvider.GetUtcNow(),
            }
        );
        return Task.CompletedTask;
    }
}
