namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>The kinds of contact point; each has one primary (BR-PTY-002).</summary>
public enum ContactPointKind
{
    /// <summary>A phone number.</summary>
    Phone,

    /// <summary>An e-mail address.</summary>
    Email,

    /// <summary>A postal address.</summary>
    Address,
}
