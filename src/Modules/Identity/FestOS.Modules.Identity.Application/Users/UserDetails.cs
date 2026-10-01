using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>A user as the edit dialog shows them; <see cref="Version"/> is also the <c>ETag</c> (api §9).</summary>
public sealed record UserDetails(
    UserId Id,
    string FullName,
    string Email,
    IReadOnlyList<Role> Roles,
    IReadOnlyList<Guid> WarehouseIds,
    bool MustChangePassword,
    DateTimeOffset? LockedUntil,
    DateTimeOffset? DeactivatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Version
);
