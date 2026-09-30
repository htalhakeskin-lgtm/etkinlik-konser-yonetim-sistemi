using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Sample.Application;

internal sealed class UseSampleItemHandler(SampleDbContext context, ExpectedVersion expectedVersion)
    : ICommandHandler<UseSampleItemCommand, bool>
{
    public async Task<bool> HandleAsync(UseSampleItemCommand command, CancellationToken cancellationToken)
    {
        SampleItem item = await context
            .SampleItems.Include(sample => sample.Parts)
            .SingleAsync(sample => sample.Id == command.SampleItemId, cancellationToken);
        expectedVersion.EnsureMatches(item);
        item.Use();
        return true;
    }
}
