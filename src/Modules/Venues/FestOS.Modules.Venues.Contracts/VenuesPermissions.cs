namespace FestOS.Modules.Venues.Contracts;

/// <summary>The permissions Venues defines (venues §6, naming §8.1).</summary>
public static class VenuesPermissions
{
    /// <summary>Lists and opens venues and their equipment.</summary>
    public const string ViewVenues = "Venues.Venues.View";

    /// <summary>Creates venues.</summary>
    public const string CreateVenues = "Venues.Venues.Create";

    /// <summary>Changes a venue's details.</summary>
    public const string EditVenues = "Venues.Venues.Edit";

    /// <summary>Deactivates and reactivates venues.</summary>
    public const string DeactivateVenues = "Venues.Venues.Deactivate";

    /// <summary>Enters a venue's equipment and its unavailability periods (US-VEN-002).</summary>
    public const string EditEquipment = "Venues.Equipment.Edit";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } =
    [ViewVenues, CreateVenues, EditVenues, DeactivateVenues, EditEquipment];
}
