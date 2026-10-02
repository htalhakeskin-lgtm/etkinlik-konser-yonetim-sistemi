using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;
using FestOS.Modules.Venues.IntegrationEvents;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Turns the domain event into the integration event, written to the outbox by the same save.</summary>
internal sealed class PublishEquipmentChangedOnVenueEquipmentChangedHandler(IOutbox outbox, TimeProvider timeProvider)
    : IDomainEventHandler<VenueEquipmentChangedDomainEvent>
{
    public Task HandleAsync(VenueEquipmentChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Add(
            new VenueEquipmentChangedIntegrationEvent
            {
                VenueId = domainEvent.VenueId.Value,
                AffectedStart = domainEvent.AffectedStart,
                AffectedEnd = domainEvent.AffectedEnd,
                OrderingKey = domainEvent.VenueId.Value,
                OccurredAt = timeProvider.GetUtcNow(),
            }
        );
        return Task.CompletedTask;
    }
}
