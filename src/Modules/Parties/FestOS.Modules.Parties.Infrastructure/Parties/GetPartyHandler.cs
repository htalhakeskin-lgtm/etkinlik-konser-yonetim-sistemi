using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Application.Parties;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

internal sealed class GetPartyHandler(PartiesDbContext context) : IQueryHandler<GetPartyQuery, PartyDetails>
{
    public async Task<PartyDetails> HandleAsync(GetPartyQuery query, CancellationToken cancellationToken) =>
        await context
            .Parties.AsNoTracking()
            .Where(party => party.Id == query.Id)
            .Select(party => new PartyDetails(
                party.Id,
                party.Kind,
                party.Name,
                party.FirstName,
                party.LastName,
                party.LegalName,
                party.Roles,
                party
                    .ContactPoints.OrderBy(point => point.SortOrder)
                    .Select(point => new ContactPointItem(
                        point.Id,
                        point.Kind,
                        point.Value,
                        point.Label,
                        point.IsPrimary
                    ))
                    .ToList(),
                party.DeactivatedAt,
                party.CreatedAt,
                party.UpdatedAt,
                party.Version
            ))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException("Party", query.Id.Value);
}
