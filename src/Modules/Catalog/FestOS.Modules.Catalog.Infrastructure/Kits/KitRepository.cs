using FestOS.BuildingBlocks.Infrastructure.Locking;
using FestOS.Modules.Catalog.Application.Kits;
using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Catalog.Infrastructure.Kits;

internal sealed class KitRepository(CatalogDbContext context) : IKitRepository
{
    // The key of the lock under which kit lines that hold other kits are read and changed (catalog CT-03).
    private const string StructureLock = "catalog:kits:structure";

    public Task<Kit?> FindAsync(KitId id, CancellationToken cancellationToken) =>
        context.Kits.Include(kit => kit.Lines).SingleOrDefaultAsync(kit => kit.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<KitId, IReadOnlyList<KitId>>> LockStructureAsync(
        CancellationToken cancellationToken
    )
    {
        await AdvisoryLocks.AcquireTransactionLocksAsync(context.Database, [StructureLock], cancellationToken);
        var held = await context
            .Kits.AsNoTracking()
            .SelectMany(
                kit => kit.Lines.Where(line => line.SubKitId != null),
                (kit, line) => new { kit.Id, line.SubKitId }
            )
            .ToListAsync(cancellationToken);
        return held.GroupBy(pair => pair.Id)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<KitId>)[.. group.Select(pair => pair.SubKitId!.Value)]
            );
    }

    public async Task<IReadOnlyDictionary<EquipmentModelId, bool>> FindModelsAsync(
        IReadOnlyCollection<EquipmentModelId> ids,
        CancellationToken cancellationToken
    )
    {
        List<EquipmentModelId> wanted = [.. ids.Distinct()];
        return await context
            .Models.AsNoTracking()
            .Where(model => wanted.Contains(model.Id))
            .ToDictionaryAsync(model => model.Id, model => model.DeactivatedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<KitId, bool>> FindKitsAsync(
        IReadOnlyCollection<KitId> ids,
        CancellationToken cancellationToken
    )
    {
        List<KitId> wanted = [.. ids.Distinct()];
        return await context
            .Kits.AsNoTracking()
            .Where(kit => wanted.Contains(kit.Id))
            .ToDictionaryAsync(kit => kit.Id, kit => kit.DeactivatedAt == null, cancellationToken);
    }

    public void Add(Kit kit) => context.Kits.Add(kit);
}
