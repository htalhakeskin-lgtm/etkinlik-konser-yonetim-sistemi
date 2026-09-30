using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.BuildingBlocks.Infrastructure.Idempotency;

/// <summary>
/// A request that was carried out, in the <c>idempotency_keys</c> table of the command's module: a retry
/// with the same key gets the stored result instead of running again (database §15, api §10).
/// </summary>
[NotAudited]
public sealed class IdempotencyKey
{
    /// <summary>The user who sent the request; keys are scoped to the user.</summary>
    public Guid UserId { get; init; }

    /// <summary>The value of the <c>Idempotency-Key</c> header.</summary>
    public Guid Key { get; init; }

    /// <summary>The SHA-256 of the request's method, address and body, as lowercase hex.</summary>
    public required string Fingerprint { get; init; }

    /// <summary>The command's result as JSON, written in the same transaction as the change.</summary>
    public string? Result { get; init; }

    /// <summary>When the request was first received.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
