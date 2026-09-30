using FestOS.BuildingBlocks.Infrastructure.Locking;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FestOS.BuildingBlocks.IntegrationTests;

/// <summary>Advisory locks as the module role uses them (database §11.3).</summary>
public sealed class AdvisoryLockTests(SampleModuleFixture fixture)
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SessionLock_HeldByOneConnection_IsRefusedToAnotherUntilReleased()
    {
        await using NpgsqlConnection first = await OpenAsync();
        await using NpgsqlConnection second = await OpenAsync();
        const string Key = "sample:test:session";

        (await AdvisoryLocks.TryAcquireSessionLockAsync(first, Key, Cancellation)).ShouldBeTrue();
        (await AdvisoryLocks.TryAcquireSessionLockAsync(second, Key, Cancellation)).ShouldBeFalse();

        await AdvisoryLocks.ReleaseSessionLockAsync(first, Key, Cancellation);

        (await AdvisoryLocks.TryAcquireSessionLockAsync(second, Key, Cancellation)).ShouldBeTrue();
        await AdvisoryLocks.ReleaseSessionLockAsync(second, Key, Cancellation);
    }

    [Fact]
    public async Task TransactionLocks_AreHeldUntilTheTransactionEnds()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        await using NpgsqlConnection other = await OpenAsync();

        await using (IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(Cancellation))
        {
            await AdvisoryLocks.AcquireTransactionLocksAsync(
                context.Database,
                ["sample:test:b", "sample:test:a"],
                Cancellation
            );

            (await TryTransactionLockAsync(other, "sample:test:a")).ShouldBeFalse();
            await transaction.CommitAsync(Cancellation);
        }

        (await TryTransactionLockAsync(other, "sample:test:a")).ShouldBeTrue();
    }

    [Fact]
    public async Task TransactionLocks_WithoutATransaction_Throw()
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            AdvisoryLocks.AcquireTransactionLocksAsync(context.Database, ["sample:test:a"], Cancellation)
        );
    }

    private async Task<NpgsqlConnection> OpenAsync()
    {
        var connection = new NpgsqlConnection(
            new NpgsqlConnectionStringBuilder(
                DatabaseConnections.ForRole(fixture.Services.GetRequiredService<IConfiguration>(), "festos_sample")
            )
            {
                Pooling = false,
            }.ConnectionString
        );
        await connection.OpenAsync(Cancellation);
        return connection;
    }

    private static async Task<bool> TryTransactionLockAsync(NpgsqlConnection connection, string key)
    {
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(Cancellation);
        await using var command = new NpgsqlCommand(
            "SELECT pg_try_advisory_xact_lock(hashtextextended($1, 0))",
            connection,
            transaction
        );
        command.Parameters.Add(new NpgsqlParameter<string> { TypedValue = key });
        bool acquired = (bool)(await command.ExecuteScalarAsync(Cancellation))!;
        await transaction.RollbackAsync(Cancellation);
        return acquired;
    }
}
