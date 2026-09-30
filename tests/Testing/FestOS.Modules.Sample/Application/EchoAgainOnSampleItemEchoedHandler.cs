using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;

namespace FestOS.Modules.Sample.Application;

/// <summary>Raises the event it handles, so the rounds never end.</summary>
internal sealed class EchoAgainOnSampleItemEchoedHandler(SampleDbContext context)
    : IDomainEventHandler<SampleItemEchoedDomainEvent>
{
    public async Task HandleAsync(SampleItemEchoedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        SampleItem item = (await context.SampleItems.FindAsync([domainEvent.SampleItemId], cancellationToken))!;
        item.Echo();
    }
}
