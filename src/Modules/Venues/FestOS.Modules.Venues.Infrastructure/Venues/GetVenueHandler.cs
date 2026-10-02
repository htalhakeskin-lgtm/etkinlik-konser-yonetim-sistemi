using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Venues.Application.Venues;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Venues.Infrastructure.Venues;

internal sealed class GetVenueHandler(VenuesDbContext context, IPartyDirectory parties)
    : IQueryHandler<GetVenueQuery, VenueDetails>
{
    public async Task<VenueDetails> HandleAsync(GetVenueQuery query, CancellationToken cancellationToken)
    {
        VenueDetails venue =
            await context
                .Venues.AsNoTracking()
                .Where(found => found.Id == query.Id)
                .Select(found => new VenueDetails(
                    found.Id,
                    found.Name,
                    found.City,
                    found.Address,
                    found.OperatorPartyId,
                    null,
                    false,
                    found.Capacity,
                    found.StageWidthMeters,
                    found.StageDepthMeters,
                    found.StageHeightMeters,
                    found.LoadingDock,
                    found.PowerCapacityAmperes,
                    found.Curfew,
                    found.TimeZone,
                    found.DeactivatedAt,
                    found.CreatedAt,
                    found.UpdatedAt,
                    found.Version
                ))
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Venue", query.Id.Value);
        if (venue.OperatorPartyId is not { } operatorId)
        {
            return venue;
        }

        IReadOnlyDictionary<Guid, PartySummary> found = await parties.FindAsync([operatorId], cancellationToken);
        return found.TryGetValue(operatorId, out PartySummary? party)
            ? venue with
            {
                OperatorName = party.Name,
                IsOperatorActive = party.IsActive,
            }
            : venue;
    }
}
