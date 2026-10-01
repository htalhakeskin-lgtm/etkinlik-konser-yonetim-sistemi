using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>
/// Changes a user's name, email, roles and warehouses (US-SYS-001). A new email ends the user's sessions
/// (ID-08); new roles or warehouses reach the open sessions in the same transaction (security §3.5).
/// </summary>
public sealed record EditUserCommand(
    UserId Id,
    string FullName,
    string Email,
    IReadOnlyList<Role> Roles,
    IReadOnlyList<Guid> WarehouseIds
) : ICommand<bool>;
