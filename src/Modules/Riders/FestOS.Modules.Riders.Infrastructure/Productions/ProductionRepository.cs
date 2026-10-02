using FestOS.Modules.Riders.Application.Productions;
using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Riders.Infrastructure.Productions;

internal sealed class ProductionRepository(RidersDbContext context) : IProductionRepository
{
    public Task<Production?> FindAsync(ProductionId id, CancellationToken cancellationToken) =>
        context.Productions.SingleOrDefaultAsync(production => production.Id == id, cancellationToken);

    public void Add(Production production, Rider rider)
    {
        context.Productions.Add(production);
        context.Riders.Add(rider);
    }
}
