namespace FestOS.Modules.Identity.Domain;

/// <summary>The rule numbers Identity reports (03 §5.1, naming §4.2).</summary>
public static class IdentityRuleCodes
{
    /// <summary>Too many wrong passwords in a row lock the account for a while.</summary>
    public const string AccountLocked = "BR-SYS-005";

    /// <summary>An email belongs to one user, active or not.</summary>
    public const string UniqueEmail = "BR-SYS-015";
}
