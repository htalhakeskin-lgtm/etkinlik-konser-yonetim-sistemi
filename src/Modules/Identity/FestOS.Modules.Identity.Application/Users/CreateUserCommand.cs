using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Opens an account for a new employee (US-SYS-001); the result carries the temporary password, once.</summary>
public sealed record CreateUserCommand(
    string FullName,
    string Email,
    IReadOnlyList<Role> Roles,
    IReadOnlyList<Guid> WarehouseIds
) : ICommand<UserCreated>;
