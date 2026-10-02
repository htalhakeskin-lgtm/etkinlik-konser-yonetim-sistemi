using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Venues.Application.Equipment;
using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

internal sealed class ListUsableVenueEquipmentHandler(VenuesDbContext context, ICatalogDirectory catalog)
    : IQueryHandler<ListUsableVenueEquipmentQuery, IReadOnlyList<UsableVenueEquipmentItem>>
{
    public async Task<IReadOnlyList<UsableVenueEquipmentItem>> HandleAsync(
        ListUsableVenueEquipmentQuery query,
        CancellationToken cancellationToken
    )
    {
        Venue venue =
            await context
                .Venues.AsNoTracking()
                .Include(found => found.Equipment)
                    .ThenInclude(line => line.Unavailabilities)
                .AsSplitQuery()
                .SingleOrDefaultAsync(found => found.Id == query.VenueId, cancellationToken)
            ?? throw new NotFoundException("Venue", query.VenueId.Value);

        // The same calculation the requirement calculation uses (venues VN-05).
        var zone = TimeZoneInfo.FindSystemTimeZoneById(venue.TimeZone);
        DateTimeOffset start = VenueDays.StartOf(query.From, zone);
        DateTimeOffset end = VenueDays.StartOf(query.To, zone);
        EquipmentNames names = await EquipmentNames.ReadAsync(catalog, venue.Equipment, cancellationToken);
        return
        [
            .. venue
                .Equipment.Select(line => new UsableVenueEquipmentItem(
                    line.Id,
                    names.Of(line).Name,
                    line.IsCounted,
                    line.Quantity,
                    line.UsableQuantity(start, end, zone)
                ))
                .OrderBy(item => item.IsCounted ? 0 : 1),
        ];
    }
}
