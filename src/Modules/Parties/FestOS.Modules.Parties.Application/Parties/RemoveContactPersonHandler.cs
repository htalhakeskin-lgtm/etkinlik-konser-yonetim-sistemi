using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class RemoveContactPersonHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<RemoveContactPersonCommand, bool>
{
    public async Task<bool> HandleAsync(RemoveContactPersonCommand command, CancellationToken cancellationToken)
    {
        Party organization = await parties.LoadForChangeAsync(
            command.OrganizationId,
            expectedVersion,
            cancellationToken
        );
        organization.EnsureHasContactPerson(command.ContactId);
        organization.RemoveContactPerson(command.ContactId);
        return true;
    }
}
