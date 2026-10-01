using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Authentication;

/// <summary>
/// The signed-in user as the front end sees them (<c>GET /api/v1/me</c>, security §3.6): the permissions
/// decide the menu and the buttons. A user who must set a new password has no permission yet (BR-SYS-006).
/// </summary>
public sealed record SignedInUserDetails(
    UserId Id,
    string FullName,
    string Email,
    IReadOnlyList<Role> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<Guid> WarehouseIds,
    bool MustChangePassword
)
{
    /// <summary>The details of a user, with the permissions of their roles unless a new password is due.</summary>
    public static SignedInUserDetails Of(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new(
            user.Id,
            user.FullName,
            user.Email,
            user.Roles,
            user.MustChangePassword ? [] : [.. RoleCatalog.PermissionsOf(user.Roles).Order(StringComparer.Ordinal)],
            user.WarehouseIds,
            user.MustChangePassword
        );
    }
}
