namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>Which productions a list shows by their status.</summary>
public enum ProductionStatusFilter
{
    /// <summary>Active productions only; the default.</summary>
    Active,

    /// <summary>Deactivated productions only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
