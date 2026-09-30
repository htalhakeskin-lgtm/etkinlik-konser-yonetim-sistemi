using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Modules.Sample.IntegrationEvents;

namespace FestOS.Modules.Sample.Application;

/// <summary>Writes a usage record for each delivered event; saved by the listener's unit of work.</summary>
internal sealed class RecordUsageOnSampleItemUsedHandler(SampleDbContext context)
    : IIntegrationEventHandler<SampleItemUsedIntegrationEvent>
{
    public Task HandleAsync(SampleItemUsedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        context.Add(SampleUsageRecord.Create(integrationEvent.SampleItemId));
        return Task.CompletedTask;
    }
}
