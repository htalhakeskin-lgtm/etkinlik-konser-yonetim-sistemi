using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Roles;

/// <summary>A permission and the roles that hold it.</summary>
public sealed record PermissionGrant(string Code, IReadOnlyList<Role> Roles);
