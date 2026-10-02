using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Venues.Application.Equipment;
using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

internal sealed class ListVenueEquipmentHandler(
    VenuesDbContext context,
    ICatalogDirectory catalog,
    TimeProvider timeProvider
) : IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList>
{
    public async Task<VenueEquipmentList> HandleAsync(
        ListVenueEquipmentQuery query,
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
        var zone = TimeZoneInfo.FindSystemTimeZoneById(venue.TimeZone);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), zone).DateTime);
        List<VenueEquipment> lines =
        [
            .. venue.Equipment.Where(line =>
                query.Status switch
                {
                    EquipmentStatusFilter.Current => line.ValidityEnd is not { } end || today < end,
                    EquipmentStatusFilter.Ended => line.ValidityEnd is { } end && end <= today,
                    _ => true,
                }
            ),
        ];
        EquipmentNames names = await EquipmentNames.ReadAsync(catalog, lines, cancellationToken);
        return new VenueEquipmentList(
            [
                .. lines
                    .Select(line =>
                    {
                        (string name, IReadOnlyList<string> path, bool isActive) = names.Of(line);
                        return new VenueEquipmentItem(
                            line.Id,
                            line.ModelId,
                            line.CategoryId,
                            line.Description,
                            name,
                            path,
                            isActive,
                            line.IsCounted,
                            line.Quantity,
                            line.ValidityStart,
                            line.ValidityEnd,
                            [
                                .. line
                                    .Unavailabilities.OrderBy(period => period.PeriodStart)
                                    .Select(period => new UnavailabilityItem(
                                        period.Id,
                                        period.PeriodStart,
                                        period.PeriodEnd,
                                        period.Quantity,
                                        period.Reason
                                    )),
                            ]
                        );
                    })
                    .OrderBy(item => item.IsCounted ? 0 : 1)
                    .ThenBy(
                        item => item.Name,
                        StringComparer.Create(
                            System.Globalization.CultureInfo.GetCultureInfo("tr-TR"),
                            ignoreCase: true
                        )
                    ),
            ],
            venue.Version
        );
    }
}
