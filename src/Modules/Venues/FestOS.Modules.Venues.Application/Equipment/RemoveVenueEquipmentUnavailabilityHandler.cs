using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class RemoveVenueEquipmentUnavailabilityHandler(
    IVenueRepository venues,
    ExpectedVersion expectedVersion
) : ICommandHandler<RemoveVenueEquipmentUnavailabilityCommand, bool>
{
    public async Task<bool> HandleAsync(
        RemoveVenueEquipmentUnavailabilityCommand command,
        CancellationToken cancellationToken
    )
    {
        Venue venue = await venues.LoadEquipmentForChangeAsync(command.VenueId, expectedVersion, cancellationToken);
        venue.EnsureHasPeriod(command.LineId, command.PeriodId);
        venue.RemoveUnavailability(command.LineId, command.PeriodId);
        return true;
    }
}
