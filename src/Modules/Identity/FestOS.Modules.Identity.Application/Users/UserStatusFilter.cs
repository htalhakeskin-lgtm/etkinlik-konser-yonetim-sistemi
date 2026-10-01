namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Which users a list shows by their status.</summary>
public enum UserStatusFilter
{
    /// <summary>Active users only; the default.</summary>
    Active,

    /// <summary>Deactivated users only.</summary>
    Inactive,

    /// <summary>Both.</summary>
    All,
}
