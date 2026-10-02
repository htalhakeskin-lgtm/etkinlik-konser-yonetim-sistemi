using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

internal sealed class AddVenueEquipmentHandler(
    IVenueRepository venues,
    ICatalogDirectory catalog,
    ExpectedVersion expectedVersion
) : ICommandHandler<AddVenueEquipmentCommand, VenueEquipmentId>
{
    public async Task<VenueEquipmentId> HandleAsync(
        AddVenueEquipmentCommand command,
        CancellationToken cancellationToken
    )
    {
        Venue venue = await venues.LoadEquipmentForChangeAsync(command.VenueId, expectedVersion, cancellationToken);
        await catalog.EnsureTargetFitsAsync(command.Details, kept: null, cancellationToken);
        return venue.AddEquipment(command.Details).Id;
    }
}
