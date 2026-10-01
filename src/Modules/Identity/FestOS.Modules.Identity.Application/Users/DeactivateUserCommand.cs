using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Closes a user's access; their sessions end at once (US-SYS-002).</summary>
public sealed record DeactivateUserCommand(UserId Id) : ICommand<bool>;
