namespace FestOS.Modules.Venues.Domain;

/// <summary>The rule numbers Venues reports (03 §5.3, naming §4.2).</summary>
public static class VenuesRuleCodes
{
    /// <summary>A venue equipment line asks for exactly one of a model, a category or a free description.</summary>
    public const string EquipmentTarget = "BR-VEN-001";

    /// <summary>A line's validity and unavailability periods hold together and do not overlap.</summary>
    public const string EquipmentPeriods = "BR-VEN-002";

    /// <summary>A venue name belongs to one venue in a city, active or not.</summary>
    public const string UniqueVenueName = "BR-VEN-003";
}
