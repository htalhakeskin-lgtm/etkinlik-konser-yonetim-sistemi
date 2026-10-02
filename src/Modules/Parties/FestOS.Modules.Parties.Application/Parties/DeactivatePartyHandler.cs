using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class DeactivatePartyHandler(
    IPartyRepository parties,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivatePartyCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivatePartyCommand command, CancellationToken cancellationToken)
    {
        Party party = await parties.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        party.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
