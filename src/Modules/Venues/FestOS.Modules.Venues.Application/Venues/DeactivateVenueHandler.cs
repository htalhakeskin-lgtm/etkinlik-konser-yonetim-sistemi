using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Venues;

internal sealed class DeactivateVenueHandler(
    IVenueRepository venues,
    ICurrentUser currentUser,
    ExpectedVersion expectedVersion,
    TimeProvider timeProvider
) : ICommandHandler<DeactivateVenueCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateVenueCommand command, CancellationToken cancellationToken)
    {
        Venue venue = await venues.LoadForChangeAsync(command.Id, expectedVersion, cancellationToken);
        venue.Deactivate(currentUser.UserId, timeProvider.GetUtcNow());
        return true;
    }
}
