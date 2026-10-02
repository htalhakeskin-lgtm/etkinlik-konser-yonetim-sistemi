using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Application.Kits;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Kits;

internal sealed class GetKitHandler(CatalogDbContext context) : IQueryHandler<GetKitQuery, KitDetails>
{
    public async Task<KitDetails> HandleAsync(GetKitQuery query, CancellationToken cancellationToken)
    {
        var kit =
            await context
                .Kits.AsNoTracking()
                .Where(found => found.Id == query.Id)
                .Select(found => new
                {
                    found.Id,
                    found.Name,
                    found.DeactivatedAt,
                    found.CreatedAt,
                    found.UpdatedAt,
                    found.Version,
                })
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Kit", query.Id.Value);
        KitCatalog catalog = await KitCatalog.ReadAsync(context, cancellationToken);
        return new KitDetails(
            kit.Id,
            kit.Name,
            catalog.LinesOf(kit.Id),
            catalog.ContentsOf(kit.Id),
            catalog.TotalsOf(kit.Id),
            kit.DeactivatedAt,
            kit.CreatedAt,
            kit.UpdatedAt,
            kit.Version
        );
    }
}
