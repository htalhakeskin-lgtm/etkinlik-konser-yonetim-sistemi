using FestOS.Modules.Riders.Application.Riders;
using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Riders.Infrastructure.Riders;

internal sealed class RiderRepository(RidersDbContext context) : IRiderRepository
{
    public Task<Rider?> FindAsync(RiderId id, CancellationToken cancellationToken) =>
        context.Riders.SingleOrDefaultAsync(rider => rider.Id == id, cancellationToken);

    public void AddVersion(RiderVersion version) => context.RiderVersions.Add(version);
}
