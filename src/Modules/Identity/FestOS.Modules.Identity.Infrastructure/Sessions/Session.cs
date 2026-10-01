using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.Modules.Identity.Domain.Users;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>
/// A signed-in browser (ADR-0011, ADR-0027). The cookie carries a random key; only its SHA-256 is kept.
/// The user's permissions and warehouses are copied in at sign-in and updated in the same transaction
/// when they change (security §3.5). A technical record, kept out of the change history.
/// </summary>
[NotAudited]
public sealed class Session
{
    /// <summary>The identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The user.</summary>
    public UserId UserId { get; init; }

    /// <summary>The SHA-256 of the cookie's key, as lowercase hex.</summary>
    public required string KeyHash { get; init; }

    /// <summary>When the user signed in.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>The last request, at most a minute old (identity ID-04).</summary>
    public DateTimeOffset LastSeenAt { get; set; }

    /// <summary>When the session ends whatever happens (P-16).</summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>The user's permissions; none while a new password is due (BR-SYS-006).</summary>
    public IList<string> Permissions { get; set; } = [];

    /// <summary>The warehouses the user was assigned when the session began.</summary>
    public IList<Guid> WarehouseIds { get; set; } = [];

    /// <summary>Whether the user must set a new password first.</summary>
    public bool MustChangePassword { get; set; }
}
