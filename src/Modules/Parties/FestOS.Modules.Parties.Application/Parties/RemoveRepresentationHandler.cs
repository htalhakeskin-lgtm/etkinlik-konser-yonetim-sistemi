using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class RemoveRepresentationHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<RemoveRepresentationCommand, bool>
{
    public async Task<bool> HandleAsync(RemoveRepresentationCommand command, CancellationToken cancellationToken)
    {
        Party artist = await parties.LoadForChangeAsync(command.ArtistId, expectedVersion, cancellationToken);
        artist.EnsureHasRepresentation(command.RepresentationId);
        artist.RemoveRepresentation(command.RepresentationId);
        return true;
    }
}
