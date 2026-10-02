using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class AddVenueEquipmentUnavailabilityHandler(IVenueRepository venues, ExpectedVersion expectedVersion)
    : ICommandHandler<AddVenueEquipmentUnavailabilityCommand, VenueEquipmentUnavailabilityId>
{
    public async Task<VenueEquipmentUnavailabilityId> HandleAsync(
        AddVenueEquipmentUnavailabilityCommand command,
        CancellationToken cancellationToken
    )
    {
        Venue venue = await venues.LoadEquipmentForChangeAsync(command.VenueId, expectedVersion, cancellationToken);
        venue.LineOf(command.LineId);
        return venue.AddUnavailability(command.LineId, command.Details).Id;
    }
}
