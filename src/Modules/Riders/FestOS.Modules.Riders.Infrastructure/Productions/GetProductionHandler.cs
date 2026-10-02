using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Riders.Application.Productions;
using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Riders.Infrastructure.Productions;

internal sealed class GetProductionHandler(RidersDbContext context, IPartyDirectory parties)
    : IQueryHandler<GetProductionQuery, ProductionDetails>
{
    public async Task<ProductionDetails> HandleAsync(GetProductionQuery query, CancellationToken cancellationToken)
    {
        Production production =
            await context
                .Productions.AsNoTracking()
                .SingleOrDefaultAsync(found => found.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Production", query.Id.Value);
        Rider rider = await context
            .Riders.AsNoTracking()
            .SingleAsync(found => found.ProductionId == query.Id, cancellationToken);

        IReadOnlyDictionary<Guid, PartySummary> artists = await parties.FindAsync(
            [production.ArtistPartyId],
            cancellationToken
        );
        PartySummary? artist = artists.GetValueOrDefault(production.ArtistPartyId);
        return new ProductionDetails(
            production.Id,
            production.ArtistPartyId,
            artist?.Name,
            artist?.IsActive ?? false,
            production.Name,
            production.Description,
            rider.Id,
            rider.Version,
            rider.LatestVersionNumber,
            production.DeactivatedAt,
            production.CreatedAt,
            production.UpdatedAt,
            production.Version
        );
    }
}
