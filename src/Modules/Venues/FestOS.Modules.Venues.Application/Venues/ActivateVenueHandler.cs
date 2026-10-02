using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

internal sealed class ActivateVenueHandler(IVenueRepository venues, ExpectedVersion expectedVersion)
    : ICommandHandler<ActivateVenueCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateVenueCommand command, CancellationToken cancellationToken)
    {
        Venue venue = await venues.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        venue.Activate();
        return true;
    }
}
