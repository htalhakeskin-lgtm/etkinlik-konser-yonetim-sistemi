using FestOS.BuildingBlocks.Application.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Idempotency;

/// <summary>A module's idempotency key table, used inside the command's transaction (api §10).</summary>
internal static class IdempotencyKeys
{
    /// <summary>
    /// Stores the key as the first step of the transaction. Returns <see langword="null"/> when no earlier
    /// request used the key, or the result the earlier request stored.
    /// </summary>
    public static async Task<string?> StoreAsync(
        DbContext context,
        Guid userId,
        Guid key,
        string fingerprint,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken
    )
    {
        int stored;
        try
        {
            // Unqualified: the module role's search_path starts with the module's schema. While an earlier
            // request with the key is still running, the insert waits for it on the primary key.
            stored = await context.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO idempotency_keys (user_id, key, fingerprint, created_at)
                VALUES ({userId}, {key}, {fingerprint}, {receivedAt})
                ON CONFLICT DO NOTHING
                """,
                cancellationToken
            );
        }
        catch (PostgresException exception) when (exception is { SqlState: PostgresErrorCodes.LockNotAvailable })
        {
            throw new IdempotencyKeyInProgressException(
                "A request with the same Idempotency-Key is still running; retry later with the same key.",
                exception
            );
        }

        if (stored == 1)
        {
            return null;
        }

        IdempotencyKey earlier = await context
            .Set<IdempotencyKey>()
            .AsNoTracking()
            .SingleAsync(row => row.UserId == userId && row.Key == key, cancellationToken);

        if (!string.Equals(earlier.Fingerprint, fingerprint, StringComparison.Ordinal))
        {
            throw new IdempotencyKeyReusedException("The Idempotency-Key was already used for a different request.");
        }

        // The result is written in the transaction that stored the key, so a committed key always has one.
        return earlier.Result
            ?? throw new InvalidOperationException($"The idempotency key {key} was stored without a result.");
    }

    /// <summary>Writes the command's result next to its key, in the same transaction as the change.</summary>
    public static Task SaveResultAsync(
        DbContext context,
        Guid userId,
        Guid key,
        string result,
        CancellationToken cancellationToken
    ) =>
        context
            .Set<IdempotencyKey>()
            .Where(row => row.UserId == userId && row.Key == key)
            .ExecuteUpdateAsync(row => row.SetProperty(stored => stored.Result, result), cancellationToken);
}
