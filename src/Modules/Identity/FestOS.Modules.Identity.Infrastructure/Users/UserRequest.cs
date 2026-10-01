using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Infrastructure.Users;

/// <summary>The body of <c>POST /api/v1/users</c> and <c>PUT /api/v1/users/{userId}</c>.</summary>
public sealed record UserRequest(
    string FullName,
    string Email,
    IReadOnlyList<Role> Roles,
    IReadOnlyList<Guid> WarehouseIds
);
