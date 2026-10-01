using System.Globalization;
using System.Security.Claims;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.Modules.Identity.Application.Authentication;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>How a session becomes the request's principal.</summary>
internal static class IdentityClaims
{
    public const string AuthenticationType = "FestOS";

    public const string Warehouse = "festos:warehouse";

    public const string MustChangePassword = "festos:must_change_password";

    public const string SessionId = "festos:session";

    public static ClaimsPrincipal PrincipalFor(SignedInUserDetails user) =>
        Principal(user.Id.Value, user.FullName, user.Permissions, user.WarehouseIds, user.MustChangePassword);

    public static ClaimsPrincipal Principal(
        Guid userId,
        string fullName,
        IEnumerable<string> permissions,
        IEnumerable<Guid> warehouseIds,
        bool mustChangePassword,
        Guid? sessionId = null
    )
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            .. permissions.Select(permission => new Claim(PermissionClaims.Type, permission)),
            .. warehouseIds.Select(warehouse => new Claim(Warehouse, warehouse.ToString())),
        ];
        if (sessionId is { } session)
        {
            claims.Add(new Claim(SessionId, session.ToString()));
        }

        if (mustChangePassword)
        {
            claims.Add(new Claim(MustChangePassword, bool.TrueString.ToLower(CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationType));
    }
}
