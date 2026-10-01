using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Users;

namespace FestOS.Modules.Identity.Infrastructure.Users;

/// <summary>The answer to a password reset: the user as they are now, and the temporary password, once.</summary>
public sealed record UserPasswordReset(UserDetails User, [property: Sensitive] string TemporaryPassword);
