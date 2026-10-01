using FestOS.BuildingBlocks.Application.Messaging;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

/// <summary>The body of <c>POST /api/v1/auth/login</c>.</summary>
public sealed record LoginRequest(string Email, [property: Sensitive] string Password);
