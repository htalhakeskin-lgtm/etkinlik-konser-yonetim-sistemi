using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Text;

namespace FestOS.Modules.Inventory.Domain.Warehouses;

/// <summary>
/// A place where equipment is kept (05 §5, US-SYS-005). Stock, transfers and reservations refer to it,
/// so it is never deleted, only deactivated (BR-SYS-001).
/// </summary>
public sealed class Warehouse : AggregateRoot<WarehouseId>, IDeactivatable
{
    /// <summary>The longest name.</summary>
    public const int NameMaxLength = 100;

    /// <summary>The longest city name.</summary>
    public const int CityMaxLength = 100;

    /// <summary>The longest address.</summary>
    public const int AddressMaxLength = 500;

    private Warehouse(WarehouseId id)
        : base(id) { }

    /// <summary>The name, unique among all warehouses (BR-SYS-016).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The name's search key (database §13): what makes two names the same, so "Merkez Depo" and
    /// "MERKEZ DEPO" are one name.
    /// </summary>
    [NotAudited]
    public string NameSearch { get; private set; } = string.Empty;

    /// <summary>The city.</summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>The address.</summary>
    public string Address { get; private set; } = string.Empty;

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>A new, active warehouse.</summary>
    public static Warehouse Create(string name, string city, string address)
    {
        var warehouse = new Warehouse(WarehouseId.New());
        warehouse.Describe(name, city, address);
        return warehouse;
    }

    /// <summary>Changes the name, city and address; the change history keeps the old values.</summary>
    public void Edit(string name, string city, string address) => Describe(name, city, address);

    /// <summary>
    /// Takes the warehouse out of new work; what already refers to it keeps its name (BR-SYS-001). Whether
    /// it is the last active one is checked by the command, which can count them (BR-SYS-013).
    /// </summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
            Raise(new WarehouseDeactivatedDomainEvent(Id));
        }
    }

    /// <summary>Opens a deactivated warehouse again (IN-03).</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }

    private void Describe(string name, string city, string address)
    {
        Name = name.Trim();
        NameSearch = SearchKey.Of(name);
        City = city.Trim();
        Address = address.Trim();
    }
}
