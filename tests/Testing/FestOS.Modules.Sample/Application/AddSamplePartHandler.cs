using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Sample.Application;

internal sealed class AddSamplePartHandler(SampleDbContext context)
    : ICommandHandler<AddSamplePartCommand, SampleItemPartId>
{
    public async Task<SampleItemPartId> HandleAsync(AddSamplePartCommand command, CancellationToken cancellationToken)
    {
        SampleItem item = await context
            .SampleItems.Include(sample => sample.Parts)
            .SingleAsync(sample => sample.Id == command.SampleItemId, cancellationToken);
        SampleItemPart part = item.AddPart(command.Label);

        if (command.FailAfterSaving)
        {
            await context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Sample failure after the part was written.");
        }

        return part.Id;
    }
}
