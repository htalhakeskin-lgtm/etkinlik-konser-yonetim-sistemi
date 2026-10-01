using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Identity.Application.Authentication;

namespace FestOS.Modules.Identity.Application.Passwords;

/// <summary>
/// Sets a new password for the signed-in user (US-SYS-011). The current password is asked again, except
/// when a temporary password is being replaced. Returns the user as <c>/me</c> shows them afterwards.
/// </summary>
public sealed record ChangeMyPasswordCommand(
    [property: Sensitive] string? CurrentPassword,
    [property: Sensitive] string NewPassword
) : ICommand<SignedInUserDetails>;
