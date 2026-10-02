using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class CreatePartyHandler(IPartyRepository parties) : ICommandHandler<CreatePartyCommand, PartyId>
{
    public Task<PartyId> HandleAsync(CreatePartyCommand command, CancellationToken cancellationToken)
    {
        Party party =
            command.Kind == PartyKind.Person
                ? Party.CreatePerson(
                    command.Name,
                    command.FirstName!,
                    command.LastName!,
                    command.Roles,
                    command.ContactPoints
                )
                : Party.CreateOrganization(command.Name, command.LegalName, command.Roles, command.ContactPoints);
        parties.Add(party);
        return Task.FromResult(party.Id);
    }
}
