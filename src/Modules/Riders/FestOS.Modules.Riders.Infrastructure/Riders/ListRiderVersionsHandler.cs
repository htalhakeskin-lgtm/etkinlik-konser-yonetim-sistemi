using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Application.Riders;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Riders.Infrastructure.Riders;

internal sealed class ListRiderVersionsHandler(RidersDbContext context)
    : IQueryHandler<ListRiderVersionsQuery, RiderVersionList>
{
    public async Task<RiderVersionList> HandleAsync(ListRiderVersionsQuery query, CancellationToken cancellationToken)
    {
        int riderVersion =
            await context
                .Riders.AsNoTracking()
                .Where(rider => rider.Id == query.RiderId)
                .Select(rider => (int?)rider.Version)
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Rider", query.RiderId.Value);
        List<RiderVersionListItem> versions = await context
            .RiderVersions.AsNoTracking()
            .Where(version => version.RiderId == query.RiderId)
            .OrderByDescending(version => version.Number)
            .Select(version => new RiderVersionListItem(
                version.Id,
                version.Number,
                version.Note,
                version.CreatedAt,
                version.CreatedByName,
                version.Lines.Count
            ))
            .ToListAsync(cancellationToken);
        return new RiderVersionList(versions, riderVersion);
    }
}
