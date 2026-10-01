using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

/// <summary>The body of <c>POST /api/v1/me/password</c>; the current password may be left out only for a temporary one.</summary>
public sealed record ChangeMyPasswordRequest(
    [property: Sensitive] string? CurrentPassword,
    [property: Sensitive] string NewPassword
);
