using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class AddContactPersonHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<AddContactPersonCommand, OrganizationContactId>
{
    public async Task<OrganizationContactId> HandleAsync(
        AddContactPersonCommand command,
        CancellationToken cancellationToken
    )
    {
        Party organization = await parties.LoadForChangeAsync(
            command.OrganizationId,
            expectedVersion,
            cancellationToken
        );
        Party person =
            await parties.FindAsync(command.PersonId, cancellationToken)
            ?? throw new NotFoundException("Party", command.PersonId.Value);
        return organization.AddContactPerson(person, command.Title).Id;
    }
}
