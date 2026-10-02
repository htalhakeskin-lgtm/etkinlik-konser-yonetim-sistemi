using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

internal sealed class DescribeRepresentationHandler(IPartyRepository parties, ExpectedVersion expectedVersion)
    : ICommandHandler<DescribeRepresentationCommand, bool>
{
    public async Task<bool> HandleAsync(DescribeRepresentationCommand command, CancellationToken cancellationToken)
    {
        Party artist = await parties.LoadForChangeAsync(command.ArtistId, expectedVersion, cancellationToken);
        artist.EnsureHasRepresentation(command.RepresentationId);
        artist.DescribeRepresentation(command.RepresentationId, command.Description);
        return true;
    }
}
