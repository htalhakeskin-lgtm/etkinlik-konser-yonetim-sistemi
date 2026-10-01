namespace FestOS.Modules.Identity.Application;

/// <summary>The permissions Identity defines (identity §5.1, naming §8.1).</summary>
public static class IdentityPermissions
{
    /// <summary>Lists and opens users.</summary>
    public const string ViewUsers = "Identity.Users.View";

    /// <summary>Creates users.</summary>
    public const string CreateUsers = "Identity.Users.Create";

    /// <summary>Changes a user's name, email, roles and warehouses.</summary>
    public const string EditUsers = "Identity.Users.Edit";

    /// <summary>Deactivates and reactivates users.</summary>
    public const string DeactivateUsers = "Identity.Users.Deactivate";

    /// <summary>Gives a user a new temporary password.</summary>
    public const string ResetUserPasswords = "Identity.Users.ResetPassword";

    /// <summary>Sees the role and permission matrix.</summary>
    public const string ViewRoles = "Identity.Roles.View";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } =
    [ViewUsers, CreateUsers, EditUsers, DeactivateUsers, ResetUserPasswords, ViewRoles];
}
