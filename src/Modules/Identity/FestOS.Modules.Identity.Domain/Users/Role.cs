namespace FestOS.Modules.Identity.Domain.Users;

/// <summary>The fixed roles of S1 (security §3.2); their permissions are defined in code.</summary>
public enum Role
{
    /// <summary>Manages users, warehouses and event defaults; sees the change history.</summary>
    SystemAdministrator,

    /// <summary>Parties, venues, productions, riders, events and holds.</summary>
    BookingManager,

    /// <summary>Catalog, requirements, reservations, transfers, conflicts and sub-rentals.</summary>
    TechnicalManager,

    /// <summary>Scanning in the assigned warehouses, unit states and the stock view.</summary>
    WarehouseManager,

    /// <summary>Sees everything and changes nothing (BR-SYS-004); sees the change history.</summary>
    GeneralManager,
}
