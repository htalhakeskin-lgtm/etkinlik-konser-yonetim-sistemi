using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.IntegrationEvents;

namespace FestOS.Modules.Sample.Application;

/// <summary>Turns the domain event into an integration event, written to the outbox by the same save.</summary>
internal sealed class PublishUsedOnSampleItemUsedHandler(IOutbox outbox, TimeProvider timeProvider)
    : IDomainEventHandler<SampleItemUsedDomainEvent>
{
    public Task HandleAsync(SampleItemUsedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Add(
            new SampleItemUsedIntegrationEvent
            {
                SampleItemId = domainEvent.SampleItemId.Value,
                OrderingKey = domainEvent.SampleItemId.Value,
                OccurredAt = timeProvider.GetUtcNow(),
            }
        );
        return Task.CompletedTask;
    }
}
