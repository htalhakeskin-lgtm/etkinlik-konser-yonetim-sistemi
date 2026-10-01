using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Roles;

/// <summary>The roles as columns and each module's permissions as rows, with the roles that hold them.</summary>
public sealed record RoleMatrix(IReadOnlyList<Role> Roles, IReadOnlyList<ModulePermissions> Modules);
