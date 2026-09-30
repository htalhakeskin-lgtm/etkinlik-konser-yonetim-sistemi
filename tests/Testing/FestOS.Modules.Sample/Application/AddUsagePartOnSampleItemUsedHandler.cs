using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;

namespace FestOS.Modules.Sample.Application;

/// <summary>Changes the aggregate from a domain event handler, inside the same save.</summary>
internal sealed class AddUsagePartOnSampleItemUsedHandler(SampleDbContext context)
    : IDomainEventHandler<SampleItemUsedDomainEvent>
{
    public async Task HandleAsync(SampleItemUsedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        SampleItem item = (await context.SampleItems.FindAsync([domainEvent.SampleItemId], cancellationToken))!;
        item.AddPart("usage");
    }
}
