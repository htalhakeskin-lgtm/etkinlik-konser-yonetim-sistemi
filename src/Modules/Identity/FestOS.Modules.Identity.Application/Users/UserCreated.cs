using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Application.Users;

/// <summary>The new user and the temporary password to hand over; shown once and never stored (BR-SYS-006).</summary>
public sealed record UserCreated(UserId Id, [property: Sensitive] string TemporaryPassword);
