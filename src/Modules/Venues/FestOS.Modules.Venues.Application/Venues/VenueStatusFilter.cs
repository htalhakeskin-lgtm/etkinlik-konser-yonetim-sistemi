namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>Which venues a list shows by their status.</summary>
public enum VenueStatusFilter
{
    /// <summary>Active venues only; the default.</summary>
    Active,

    /// <summary>Deactivated venues only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
