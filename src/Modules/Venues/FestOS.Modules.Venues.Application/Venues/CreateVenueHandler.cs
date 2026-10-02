using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

internal sealed class CreateVenueHandler(IVenueRepository venues, IPartyDirectory parties)
    : ICommandHandler<CreateVenueCommand, VenueId>
{
    public async Task<VenueId> HandleAsync(CreateVenueCommand command, CancellationToken cancellationToken)
    {
        await parties.EnsureSelectableOperatorAsync(command.Description.OperatorPartyId, cancellationToken);
        var venue = Venue.Create(command.Description);
        venues.Add(venue);
        return venue.Id;
    }
}
