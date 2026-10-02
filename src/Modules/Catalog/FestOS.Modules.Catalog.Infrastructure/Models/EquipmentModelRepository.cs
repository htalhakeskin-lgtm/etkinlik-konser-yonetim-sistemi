using FestOS.Modules.Catalog.Application.Models;
using FestOS.Modules.Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Models;

internal sealed class EquipmentModelRepository(CatalogDbContext context) : IEquipmentModelRepository
{
    public Task<EquipmentModel?> FindAsync(EquipmentModelId id, CancellationToken cancellationToken) =>
        context.Models.SingleOrDefaultAsync(model => model.Id == id, cancellationToken);

    public void Add(EquipmentModel model) => context.Models.Add(model);
}
