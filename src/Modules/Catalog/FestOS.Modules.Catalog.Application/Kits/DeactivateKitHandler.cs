using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Catalog.Domain.Kits;

namespace FestOS.Modules.Catalog.Application.Kits;

internal sealed class DeactivateKitHandler(
    IKitRepository kits,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivateKitCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateKitCommand command, CancellationToken cancellationToken)
    {
        Kit kit = await kits.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        kit.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
