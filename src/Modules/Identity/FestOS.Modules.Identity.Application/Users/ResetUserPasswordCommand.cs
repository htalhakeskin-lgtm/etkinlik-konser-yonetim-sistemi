using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>Gives a user a new temporary password, lifts a lock and ends their sessions (US-SYS-002).</summary>
public sealed record ResetUserPasswordCommand(UserId Id) : ICommand<TemporaryPasswordIssued>;
