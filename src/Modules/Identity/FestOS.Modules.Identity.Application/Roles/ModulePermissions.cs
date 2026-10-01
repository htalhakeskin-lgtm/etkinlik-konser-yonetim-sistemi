namespace FestOS.Modules.Identity.Application.Roles;

/// <summary>A module's permissions, in the order the module declares them.</summary>
public sealed record ModulePermissions(string Module, IReadOnlyList<PermissionGrant> Permissions);
