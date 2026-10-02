namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>Which kits a list shows by their status.</summary>
public enum KitStatusFilter
{
    /// <summary>Active kits only; the default.</summary>
    Active,

    /// <summary>Deactivated kits only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
