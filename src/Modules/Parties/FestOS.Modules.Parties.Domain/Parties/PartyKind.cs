namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>Whether a party is a real person or a legal entity (01 §3.2); fixed once created (BR-PTY-001).</summary>
public enum PartyKind
{
    /// <summary>A real person.</summary>
    Person,

    /// <summary>A company or another legal entity.</summary>
    Organization,
}
