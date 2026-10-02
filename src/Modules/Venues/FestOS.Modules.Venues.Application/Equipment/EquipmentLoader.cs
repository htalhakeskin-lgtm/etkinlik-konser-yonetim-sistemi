using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>
/// Loads the venue whose equipment a command changes, at the version the request saw (api §9, venues
/// VN-04); a line or period named in the address belongs to it, or the answer is 404.
/// </summary>
internal static class EquipmentLoader
{
    public static async Task<Venue> LoadEquipmentForChangeAsync(
        this IVenueRepository venues,
        VenueId id,
        ExpectedVersion expectedVersion,
        CancellationToken cancellationToken
    )
    {
        Venue venue =
            await venues.FindWithEquipmentAsync(id, cancellationToken)
            ?? throw new NotFoundException("Venue", id.Value);
        expectedVersion.EnsureMatches(venue);
        return venue;
    }

    public static VenueEquipment LineOf(this Venue venue, VenueEquipmentId lineId) =>
        venue.Equipment.FirstOrDefault(line => line.Id == lineId)
        ?? throw new NotFoundException("VenueEquipment", lineId.Value);

    public static void EnsureHasPeriod(
        this Venue venue,
        VenueEquipmentId lineId,
        VenueEquipmentUnavailabilityId periodId
    )
    {
        if (!venue.LineOf(lineId).Unavailabilities.Any(period => period.Id == periodId))
        {
            throw new NotFoundException("VenueEquipmentUnavailability", periodId.Value);
        }
    }
}
