using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

internal sealed class DeactivateProductionHandler(
    IProductionRepository productions,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivateProductionCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateProductionCommand command, CancellationToken cancellationToken)
    {
        Production production = await productions.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        production.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
