using System.Net;
using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

/// <summary>
/// One sign-in attempt (security §4.1). A technical record, kept out of the change history and deleted
/// after 90 days since it holds personal data (security §10).
/// </summary>
[NotAudited]
public sealed class LoginAttempt
{
    /// <summary>The identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The email as typed, in its stored form.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>The user with that email, when there is one.</summary>
    public UserId? UserId { get; init; }

    /// <summary>When the attempt was made.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Whether it opened a session.</summary>
    public bool Succeeded { get; init; }

    /// <summary>The address the request came from, when known.</summary>
    public IPAddress? IpAddress { get; init; }
}
