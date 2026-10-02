using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Riders.Application.Productions;
using FestOS.Modules.Riders.Domain.Productions;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Riders.Infrastructure.Productions;

internal sealed class ListProductionsHandler(RidersDbContext context, IPartyDirectory parties)
    : IQueryHandler<ListProductionsQuery, PagedResult<ProductionListItem>>
{
    public static SortKeys<Production> SortKeys { get; } =
        new SortKeys<Production>()
            .Add("name", production => EF.Functions.Collate(production.Name, Collations.Turkish))
            .Add("createdAt", production => production.CreatedAt);

    public async Task<PagedResult<ProductionListItem>> HandleAsync(
        ListProductionsQuery query,
        CancellationToken cancellationToken
    )
    {
        IQueryable<Production> productions = context.Productions.AsNoTracking();
        productions = query.Status switch
        {
            ProductionStatusFilter.Active => productions.Where(production => production.DeactivatedAt == null),
            ProductionStatusFilter.Inactive => productions.Where(production => production.DeactivatedAt != null),
            _ => productions,
        };
        if (query.ArtistId is { } artistId)
        {
            productions = productions.Where(production => production.ArtistPartyId == artistId);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string key = SearchKey.Of(query.Q);
            productions = productions.Where(production => production.NameSearch.Contains(key));
        }

        PagedResult<ProductionListItem> page = await SortKeys
            .Apply(
                productions,
                SortSpec.Parse(query.Sort, ListProductionsQuery.DefaultSort),
                production => production.Id
            )
            .Select(production => new ProductionListItem(
                production.Id,
                production.ArtistPartyId,
                null,
                production.Name,
                context
                    .Riders.Where(rider => rider.ProductionId == production.Id)
                    .Select(rider => rider.LatestVersionNumber)
                    .First(),
                context
                    .Riders.Where(rider => rider.ProductionId == production.Id)
                    .SelectMany(rider =>
                        context.RiderVersions.Where(version =>
                            version.RiderId == rider.Id && version.Number == rider.LatestVersionNumber
                        )
                    )
                    .Select(version => (DateTimeOffset?)version.CreatedAt)
                    .FirstOrDefault(),
                production.DeactivatedAt == null,
                production.Version
            ))
            .ToPagedResultAsync(query.Page, cancellationToken);

        // The artists' names come from Parties in one call (parties MD-02).
        IReadOnlyDictionary<Guid, PartySummary> artists = await parties.FindAsync(
            [.. page.Items.Select(item => item.ArtistPartyId).Distinct()],
            cancellationToken
        );
        return page with
        {
            Items =
            [
                .. page.Items.Select(item =>
                    item with
                    {
                        ArtistName = artists.TryGetValue(item.ArtistPartyId, out PartySummary? artist)
                            ? artist.Name
                            : null,
                    }
                ),
            ],
        };
    }
}
