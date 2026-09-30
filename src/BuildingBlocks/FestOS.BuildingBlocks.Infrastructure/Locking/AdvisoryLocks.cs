using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Locking;

/// <summary>
/// PostgreSQL advisory locks (database §11.3). Keys are shared by the whole database, so each starts
/// with <c>{module}:{purpose}:</c>; a key is turned into a lock number with <c>hashtextextended</c>.
/// </summary>
public static class AdvisoryLocks
{
    /// <summary>
    /// Takes locks that are released when the current transaction ends, in a fixed order so two
    /// transactions asking for the same keys cannot deadlock. Call it inside the unit of work.
    /// </summary>
    public static async Task AcquireTransactionLocksAsync(
        DatabaseFacade database,
        IEnumerable<string> keys,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(database);
        if (database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Transaction-level locks need the unit of work's transaction.");
        }

        foreach (string key in keys.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            await database.ExecuteSqlAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))",
                cancellationToken
            );
        }
    }

    /// <summary>
    /// Tries to take a session lock without waiting. Only for the single run of scheduled jobs, on a
    /// connection of their own that is not pooled, released in <c>finally</c> (database §11.3).
    /// </summary>
    public static async Task<bool> TryAcquireSessionLockAsync(
        NpgsqlConnection connection,
        string key,
        CancellationToken cancellationToken
    )
    {
        await using NpgsqlCommand command = LockCommand(
            "SELECT pg_try_advisory_lock(hashtextextended($1, 0))",
            connection,
            key
        );
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    /// <summary>Releases a session lock taken by <see cref="TryAcquireSessionLockAsync"/>.</summary>
    public static async Task ReleaseSessionLockAsync(
        NpgsqlConnection connection,
        string key,
        CancellationToken cancellationToken
    )
    {
        await using NpgsqlCommand command = LockCommand(
            "SELECT pg_advisory_unlock(hashtextextended($1, 0))",
            connection,
            key
        );
        await command.ExecuteScalarAsync(cancellationToken);
    }

    private static NpgsqlCommand LockCommand(string sql, NpgsqlConnection connection, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter<string> { TypedValue = key });
        return command;
    }
}
