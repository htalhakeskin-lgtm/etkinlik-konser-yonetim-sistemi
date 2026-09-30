using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;

namespace FestOS.Modules.Sample.Application;

/// <summary>Adds the item and returns; the unit of work saves.</summary>
internal sealed class CreateSampleItemHandler(SampleDbContext context)
    : ICommandHandler<CreateSampleItemCommand, SampleItemId>
{
    public Task<SampleItemId> HandleAsync(CreateSampleItemCommand command, CancellationToken cancellationToken)
    {
        var item = SampleItem.Create(command.Name, command.UnitPrice);
        context.SampleItems.Add(item);
        return Task.FromResult(item.Id);
    }
}
