using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class EditPartyHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<EditPartyCommand, bool>
{
    public async Task<bool> HandleAsync(EditPartyCommand command, CancellationToken cancellationToken)
    {
        Party party = await parties.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        if (party.Kind == PartyKind.Person)
        {
            EnsurePersonNames(command);
        }

        party.Edit(
            command.Name,
            command.FirstName,
            command.LastName,
            command.LegalName,
            command.Roles,
            command.ContactPoints
        );
        return true;
    }

    // A person keeps a first and last name; the request does not say the kind, so it is checked here.
    private static void EnsurePersonNames(EditPartyCommand command)
    {
        List<ValidationError> errors =
        [
            .. new[] { ("FirstName", command.FirstName), ("LastName", command.LastName) }
                .Where(name => string.IsNullOrWhiteSpace(name.Item2))
                .Select(name => new ValidationError(
                    name.Item1,
                    "NotEmptyValidator",
                    new Dictionary<string, object?>(StringComparer.Ordinal)
                )),
        ];
        if (errors.Count > 0)
        {
            throw new ValidationFailedException(errors);
        }
    }
}
