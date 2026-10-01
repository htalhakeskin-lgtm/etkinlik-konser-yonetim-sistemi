namespace FestOS.Modules.Inventory.Contracts;

/// <summary>
/// Which warehouses exist and are active, for the modules that refer to warehouses (inventory §6); Identity
/// checks a warehouse manager's warehouses with it (BR-SYS-014).
/// </summary>
public interface IWarehouseDirectory
{
    /// <summary>The identifiers among <paramref name="ids"/> that belong to existing, active warehouses.</summary>
    Task<IReadOnlySet<Guid>> FindActiveAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}
