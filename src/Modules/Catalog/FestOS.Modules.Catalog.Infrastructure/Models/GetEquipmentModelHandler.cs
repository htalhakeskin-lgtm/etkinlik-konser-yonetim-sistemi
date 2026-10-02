using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Application.Models;
using FestOS.Modules.Catalog.Infrastructure.Categories;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Models;

internal sealed class GetEquipmentModelHandler(CatalogDbContext context)
    : IQueryHandler<GetEquipmentModelQuery, EquipmentModelDetails>
{
    public async Task<EquipmentModelDetails> HandleAsync(
        GetEquipmentModelQuery query,
        CancellationToken cancellationToken
    )
    {
        EquipmentModelDetails model =
            await context
                .Models.AsNoTracking()
                .Where(found => found.Id == query.Id)
                .Select(found => new EquipmentModelDetails(
                    found.Id,
                    found.Brand,
                    found.Name,
                    found.CategoryId,
                    Array.Empty<string>(),
                    found.TrackingType,
                    found.WeightKilograms,
                    found.PowerWatts,
                    found.TransportVolumeCubicMeters,
                    found.HasStock,
                    found.DeactivatedAt,
                    found.CreatedAt,
                    found.UpdatedAt,
                    found.Version
                ))
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("EquipmentModel", query.Id.Value);
        CategoryPaths paths = await CategoryPaths.ReadAsync(context, cancellationToken);
        return model with { CategoryPath = paths.PathOf(model.CategoryId) };
    }
}
