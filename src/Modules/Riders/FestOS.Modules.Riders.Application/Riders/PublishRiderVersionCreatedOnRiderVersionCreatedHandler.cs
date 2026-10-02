using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.RiderVersions;
using FestOS.Modules.Riders.IntegrationEvents;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>Turns the domain event into the integration event, written to the outbox by the same save.</summary>
internal sealed class PublishRiderVersionCreatedOnRiderVersionCreatedHandler(IOutbox outbox, TimeProvider timeProvider)
    : IDomainEventHandler<RiderVersionCreatedDomainEvent>
{
    public Task HandleAsync(RiderVersionCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Add(
            new RiderVersionCreatedIntegrationEvent
            {
                RiderId = domainEvent.RiderId.Value,
                RiderVersionId = domainEvent.RiderVersionId.Value,
                Number = domainEvent.Number,
                ProductionId = domainEvent.ProductionId?.Value,
                OrderingKey = domainEvent.RiderId.Value,
                OccurredAt = timeProvider.GetUtcNow(),
            }
        );
        return Task.CompletedTask;
    }
}
