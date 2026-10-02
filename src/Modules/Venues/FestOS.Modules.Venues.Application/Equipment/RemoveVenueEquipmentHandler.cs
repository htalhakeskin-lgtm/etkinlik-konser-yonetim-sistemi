using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class RemoveVenueEquipmentHandler(IVenueRepository venues, ExpectedVersion expectedVersion)
    : ICommandHandler<RemoveVenueEquipmentCommand, bool>
{
    public async Task<bool> HandleAsync(RemoveVenueEquipmentCommand command, CancellationToken cancellationToken)
    {
        Venue venue = await venues.LoadEquipmentForChangeAsync(command.VenueId, expectedVersion, cancellationToken);
        venue.LineOf(command.LineId);
        venue.RemoveEquipment(command.LineId);
        return true;
    }
}
