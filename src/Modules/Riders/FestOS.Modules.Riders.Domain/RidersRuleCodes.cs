namespace FestOS.Modules.Riders.Domain;

/// <summary>The rule numbers Riders reports (03 §5.5, naming §4.2).</summary>
public static class RidersRuleCodes
{
    /// <summary>A rider's source matches what it belongs to: a production's rider comes from the production.</summary>
    public const string RiderSource = "BR-RDR-007";

    /// <summary>A production name belongs to one production of an artist, active or not.</summary>
    public const string UniqueProductionName = "BR-RDR-009";
}
