namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>Which models a list shows by their status.</summary>
public enum ModelStatusFilter
{
    /// <summary>Active models only; the default.</summary>
    Active,

    /// <summary>Deactivated models only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
