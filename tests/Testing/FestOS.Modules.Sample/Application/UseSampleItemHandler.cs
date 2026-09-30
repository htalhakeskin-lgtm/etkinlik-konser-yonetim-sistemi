using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Sample.Application;

internal sealed class UseSampleItemHandler(SampleDbContext context) : ICommandHandler<UseSampleItemCommand, bool>
{
    public async Task<bool> HandleAsync(UseSampleItemCommand command, CancellationToken cancellationToken)
    {
        (
            await context
                .SampleItems.Include(sample => sample.Parts)
                .SingleAsync(sample => sample.Id == command.SampleItemId, cancellationToken)
        ).Use();
        return true;
    }
}
