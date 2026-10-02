using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class EditVenueEquipmentUnavailabilityHandler(IVenueRepository venues, ExpectedVersion expectedVersion)
    : ICommandHandler<EditVenueEquipmentUnavailabilityCommand, bool>
{
    public async Task<bool> HandleAsync(
        EditVenueEquipmentUnavailabilityCommand command,
        CancellationToken cancellationToken
    )
    {
        Venue venue = await venues.LoadEquipmentForChangeAsync(command.VenueId, expectedVersion, cancellationToken);
        venue.EnsureHasPeriod(command.LineId, command.PeriodId);
        venue.EditUnavailability(command.LineId, command.PeriodId, command.Details);
        return true;
    }
}
