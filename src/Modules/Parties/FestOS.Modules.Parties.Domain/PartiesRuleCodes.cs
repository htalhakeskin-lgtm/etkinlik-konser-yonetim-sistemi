namespace FestOS.Modules.Parties.Domain;

/// <summary>The rule numbers Parties reports (03 §5.2, naming §4.2).</summary>
public static class PartiesRuleCodes
{
    /// <summary>A party has at least one role, each once, and keeps its kind.</summary>
    public const string PartyRoles = "BR-PTY-001";

    /// <summary>Each kind of contact point a party has has exactly one primary.</summary>
    public const string PrimaryContactPoint = "BR-PTY-002";
}
