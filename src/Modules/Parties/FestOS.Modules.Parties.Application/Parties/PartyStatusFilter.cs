namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Which parties a list shows by their status.</summary>
public enum PartyStatusFilter
{
    /// <summary>Active parties only; the default.</summary>
    Active,

    /// <summary>Deactivated parties only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
