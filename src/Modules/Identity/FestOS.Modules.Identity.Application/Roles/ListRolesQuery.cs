using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Identity.Application.Roles;

/// <summary>The role and permission matrix, grouped by module (US-SYS-003); read-only in S1.</summary>
public sealed record ListRolesQuery : IQuery<RoleMatrix>;
