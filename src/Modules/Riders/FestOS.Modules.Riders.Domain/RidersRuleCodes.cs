namespace FestOS.Modules.Riders.Domain;

/// <summary>The rule numbers Riders reports (03 §5.5, naming §4.2).</summary>
public static class RidersRuleCodes
{
    /// <summary>A rider line has exactly one target, a quantity of at least one, and flexibility on model lines only.</summary>
    public const string LineTarget = "BR-RDR-001";

    /// <summary>Only a flexible model line has equivalents: other, distinct, active models.</summary>
    public const string EquivalentModels = "BR-RDR-002";

    /// <summary>Versions are numbered one after another and never change.</summary>
    public const string VersionNumbers = "BR-RDR-003";

    /// <summary>A rider's source matches what it belongs to: a production's rider comes from the production.</summary>
    public const string RiderSource = "BR-RDR-007";

    /// <summary>A production name belongs to one production of an artist, active or not.</summary>
    public const string UniqueProductionName = "BR-RDR-009";
}
