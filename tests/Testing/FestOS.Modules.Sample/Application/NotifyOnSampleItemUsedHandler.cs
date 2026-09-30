using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.IntegrationEvents;

namespace FestOS.Modules.Sample.Application;

/// <summary>A second listener of the same event, which a test can make fail.</summary>
internal sealed class NotifyOnSampleItemUsedHandler(SampleListenerProbe probe)
    : IIntegrationEventHandler<SampleItemUsedIntegrationEvent>
{
    public Task HandleAsync(SampleItemUsedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        probe.Run();
        return Task.CompletedTask;
    }
}
