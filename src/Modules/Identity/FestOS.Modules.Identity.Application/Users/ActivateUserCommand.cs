using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Opens a deactivated user's access again (ID-05).</summary>
public sealed record ActivateUserCommand(UserId Id) : ICommand<bool>;
