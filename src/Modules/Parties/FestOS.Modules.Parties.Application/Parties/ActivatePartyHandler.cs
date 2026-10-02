using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class ActivatePartyHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<ActivatePartyCommand, bool>
{
    public async Task<bool> HandleAsync(ActivatePartyCommand command, CancellationToken cancellationToken)
    {
        Party party = await parties.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        party.Activate();
        return true;
    }
}
