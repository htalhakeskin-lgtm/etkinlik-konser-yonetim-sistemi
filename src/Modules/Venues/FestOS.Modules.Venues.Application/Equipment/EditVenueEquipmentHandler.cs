using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class EditVenueEquipmentHandler(
    IVenueRepository venues,
    ICatalogDirectory catalog,
    ExpectedVersion expectedVersion
) : ICommandHandler<EditVenueEquipmentCommand, bool>
{
    public async Task<bool> HandleAsync(EditVenueEquipmentCommand command, CancellationToken cancellationToken)
    {
        Venue venue = await venues.LoadEquipmentForChangeAsync(command.VenueId, expectedVersion, cancellationToken);
        await catalog.EnsureTargetFitsAsync(command.Details, venue.LineOf(command.LineId), cancellationToken);
        venue.EditEquipment(command.LineId, command.Details);
        return true;
    }
}
