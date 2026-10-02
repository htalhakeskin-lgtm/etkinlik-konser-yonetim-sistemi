using FestOS.BuildingBlocks.Infrastructure.Locking;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Categories;

internal sealed class EquipmentCategoryRepository(CatalogDbContext context) : IEquipmentCategoryRepository
{
    // The key of the lock under which the tree is read and changed (catalog CT-03, database §11.3).
    private const string TreeLock = "catalog:categories:tree";

    public Task<EquipmentCategory?> FindAsync(EquipmentCategoryId id, CancellationToken cancellationToken) =>
        context.Categories.SingleOrDefaultAsync(category => category.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CategoryNode>> LockTreeAsync(CancellationToken cancellationToken)
    {
        await AdvisoryLocks.AcquireTransactionLocksAsync(context.Database, [TreeLock], cancellationToken);
        return await context
            .Categories.AsNoTracking()
            .Select(category => new CategoryNode(category.Id, category.ParentId, category.DeactivatedAt == null))
            .ToListAsync(cancellationToken);
    }

    public void Add(EquipmentCategory category) => context.Categories.Add(category);
}
