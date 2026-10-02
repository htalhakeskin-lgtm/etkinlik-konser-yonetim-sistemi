using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class AddRepresentationHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<AddRepresentationCommand, ArtistRepresentationId>
{
    public async Task<ArtistRepresentationId> HandleAsync(
        AddRepresentationCommand command,
        CancellationToken cancellationToken
    )
    {
        Party artist = await parties.LoadForChangeAsync(command.ArtistId, expectedVersion, cancellationToken);
        Party agency =
            await parties.FindAsync(command.AgencyId, cancellationToken)
            ?? throw new NotFoundException("Party", command.AgencyId.Value);
        return artist.AddRepresentation(agency, command.Description).Id;
    }
}
