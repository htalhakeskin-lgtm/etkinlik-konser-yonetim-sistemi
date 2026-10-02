using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Parties.Application.Parties;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

internal sealed class ListPartiesHandler(PartiesDbContext context)
    : IQueryHandler<ListPartiesQuery, PagedResult<PartyListItem>>
{
    public static SortKeys<Party> SortKeys { get; } =
        new SortKeys<Party>()
            .Add("name", party => EF.Functions.Collate(party.Name, Collations.Turkish))
            .Add("createdAt", party => party.CreatedAt);

    public Task<PagedResult<PartyListItem>> HandleAsync(ListPartiesQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Party> parties = context.Parties.AsNoTracking();
        parties = query.Status switch
        {
            PartyStatusFilter.Active => parties.Where(party => party.DeactivatedAt == null),
            PartyStatusFilter.Inactive => parties.Where(party => party.DeactivatedAt != null),
            _ => parties,
        };
        if (query.Role is { } role)
        {
            parties = parties.Where(party => party.Roles.Contains(role));
        }

        if (query.Kind is { } kind)
        {
            parties = parties.Where(party => party.Kind == kind);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            string key = SearchKey.Of(query.Q);
            parties = parties.Where(party => party.Search.Contains(key));
        }

        return SortKeys
            .Apply(parties, SortSpec.Parse(query.Sort, ListPartiesQuery.DefaultSort), party => party.Id)
            .Select(party => new PartyListItem(
                party.Id,
                party.Kind,
                party.Name,
                party.Roles,
                party
                    .ContactPoints.Where(point => point.Kind == ContactPointKind.Phone && point.IsPrimary)
                    .Select(point => point.Value)
                    .FirstOrDefault(),
                party
                    .ContactPoints.Where(point => point.Kind == ContactPointKind.Email && point.IsPrimary)
                    .Select(point => point.Value)
                    .FirstOrDefault(),
                party.DeactivatedAt == null,
                party.Version
            ))
            .ToPagedResultAsync(query.Page, cancellationToken);
    }
}
