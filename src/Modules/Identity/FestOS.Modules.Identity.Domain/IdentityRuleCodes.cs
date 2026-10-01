namespace FestOS.Modules.Identity.Domain;

/// <summary>The rule numbers Identity reports (03 §5.1, naming §4.2).</summary>
public static class IdentityRuleCodes
{
    /// <summary>Too many wrong passwords in a row lock the account for a while.</summary>
    public const string AccountLocked = "BR-SYS-005";

    /// <summary>A new password meets the password policy.</summary>
    public const string PasswordPolicy = "BR-SYS-007";

    /// <summary>A system administrator keeps the system administered: not by themselves, not the last one.</summary>
    public const string AdministratorProtection = "BR-SYS-009";

    /// <summary>A warehouse manager has at least one warehouse.</summary>
    public const string WarehouseManagerHasWarehouse = "BR-SYS-014";

    /// <summary>An email belongs to one user, active or not.</summary>
    public const string UniqueEmail = "BR-SYS-015";
}
