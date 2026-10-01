using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>A row of the users list; <c>LockedUntil</c> is set while sign-in is refused (BR-SYS-005).</summary>
public sealed record UserListItem(
    UserId Id,
    string FullName,
    string Email,
    IReadOnlyList<Role> Roles,
    bool IsActive,
    DateTimeOffset? LockedUntil
);
