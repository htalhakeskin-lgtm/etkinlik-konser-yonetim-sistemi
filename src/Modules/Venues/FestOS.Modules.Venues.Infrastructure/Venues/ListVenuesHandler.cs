using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Venues.Infrastructure.Venues;

internal sealed class ListVenuesHandler(VenuesDbContext context, IPartyDirectory parties)
    : IQueryHandler<ListVenuesQuery, PagedResult<VenueListItem>>
{
    public static SortKeys<Venue> SortKeys { get; } =
        new SortKeys<Venue>()
            .Add("name", venue => EF.Functions.Collate(venue.Name, Collations.Turkish))
            .Add("city", venue => EF.Functions.Collate(venue.City, Collations.Turkish))
            .Add("capacity", venue => venue.Capacity);

    public async Task<PagedResult<VenueListItem>> HandleAsync(
        ListVenuesQuery query,
        CancellationToken cancellationToken
    )
    {
        IQueryable<Venue> venues = context.Venues.AsNoTracking();
        venues = query.Status switch
        {
            VenueStatusFilter.Active => venues.Where(venue => venue.DeactivatedAt == null),
            VenueStatusFilter.Inactive => venues.Where(venue => venue.DeactivatedAt != null),
            _ => venues,
        };
        if (!string.IsNullOrWhiteSpace(query.City))
        {
            string city = SearchKey.Of(query.City);
            venues = venues.Where(venue => venue.CitySearch == city);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string key = SearchKey.Of(query.Q);
            venues = venues.Where(venue => venue.NameSearch.Contains(key) || venue.CitySearch.Contains(key));
        }

        PagedResult<VenueListItem> page = await SortKeys
            .Apply(venues, SortSpec.Parse(query.Sort, ListVenuesQuery.DefaultSort), venue => venue.Id)
            .Select(venue => new VenueListItem(
                venue.Id,
                venue.Name,
                venue.City,
                venue.Capacity,
                venue.OperatorPartyId,
                null,
                venue.DeactivatedAt == null,
                venue.Version
            ))
            .ToPagedResultAsync(query.Page, cancellationToken);

        // The operators' names come from Parties in one call (parties MD-02).
        IReadOnlyDictionary<Guid, PartySummary> operators = await parties.FindAsync(
            [.. page.Items.Select(item => item.OperatorPartyId).OfType<Guid>()],
            cancellationToken
        );
        return page with
        {
            Items =
            [
                .. page.Items.Select(item =>
                    item with
                    {
                        OperatorName =
                            item.OperatorPartyId is { } id && operators.TryGetValue(id, out PartySummary? party)
                                ? party.Name
                                : null,
                    }
                ),
            ],
        };
    }
}
