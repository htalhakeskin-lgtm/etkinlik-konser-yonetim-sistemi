namespace FestOS.BuildingBlocks.Infrastructure.Authorization;

/// <summary>
/// The claim that carries one permission of the signed-in user (security §3). Identity puts the union
/// of the user's role permissions into the session and the principal (BR-SYS-002 is applied there).
/// </summary>
public static class PermissionClaims
{
    /// <summary>The claim type; the value is a permission code such as <c>Identity.Users.View</c>.</summary>
    public const string Type = "festos:permission";
}
