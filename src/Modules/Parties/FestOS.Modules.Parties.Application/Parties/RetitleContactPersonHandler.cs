using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class RetitleContactPersonHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<RetitleContactPersonCommand, bool>
{
    public async Task<bool> HandleAsync(RetitleContactPersonCommand command, CancellationToken cancellationToken)
    {
        Party organization = await parties.LoadForChangeAsync(
            command.OrganizationId,
            expectedVersion,
            cancellationToken
        );
        organization.EnsureHasContactPerson(command.ContactId);
        organization.RetitleContactPerson(command.ContactId, command.Title);
        return true;
    }
}
