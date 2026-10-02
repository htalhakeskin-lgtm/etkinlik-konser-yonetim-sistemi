using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>Loads and adds kits for commands (identity ID-01); the unit of work saves them.</summary>
public interface IKitRepository
{
    /// <summary>The kit with its lines, or <see langword="null"/>.</summary>
    Task<Kit?> FindAsync(KitId id, CancellationToken cancellationToken);

    /// <summary>
    /// Takes the lock on the kits' structure and reads which kits each kit holds, so two kits that add each
    /// other at once cannot both miss the cycle (catalog CT-03).
    /// </summary>
    Task<IReadOnlyDictionary<KitId, IReadOnlyList<KitId>>> LockStructureAsync(CancellationToken cancellationToken);

    /// <summary>Which of the models exist, and whether each is active.</summary>
    Task<IReadOnlyDictionary<EquipmentModelId, bool>> FindModelsAsync(
        IReadOnlyCollection<EquipmentModelId> ids,
        CancellationToken cancellationToken
    );

    /// <summary>Which of the kits exist, and whether each is active.</summary>
    Task<IReadOnlyDictionary<KitId, bool>> FindKitsAsync(
        IReadOnlyCollection<KitId> ids,
        CancellationToken cancellationToken
    );

    /// <summary>Adds a new kit.</summary>
    void Add(Kit kit);
}
