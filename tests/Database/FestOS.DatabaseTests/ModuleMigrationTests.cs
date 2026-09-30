using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace FestOS.DatabaseTests;

/// <summary>The module migrations against a bootstrapped database (database §16, §17.1).</summary>
public sealed class ModuleMigrationTests(PostgresDatabase database)
{
    private readonly Dictionary<string, string> _passwords = new(StringComparer.Ordinal)
    {
        [DatabaseRoles.Migrator] = Guid.CreateVersion7().ToString("N"),
        ["festos_sample"] = Guid.CreateVersion7().ToString("N"),
    };

    [Fact]
    [Trait("DatabaseRule", "DT-01")]
    public async Task Migrations_ForEveryModule_ApplyWithTheMigratorRole()
    {
        await BootstrapAsync();

        foreach (ModuleDbContext context in ModuleContexts.Create(ConnectionString(DatabaseRoles.Migrator)))
        {
            await using (context)
            {
                await context.Database.MigrateAsync(TestContext.Current.CancellationToken);

                (
                    await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken)
                ).ShouldBeEmpty();
            }
        }
    }

    [Fact]
    [Trait("DatabaseRule", "DT-01")]
    public void Model_ForEveryModule_HasNoChangesMissingFromTheMigrations()
    {
        foreach (ModuleDbContext context in ModuleContexts.Create(ModuleContexts.ModelOnlyConnectionString))
        {
            using (context)
            {
                context.Database.HasPendingModelChanges().ShouldBeFalse(context.GetType().Name);
            }
        }
    }

    [Fact]
    [Trait("DatabaseRule", "DT-02")]
    public async Task ModuleRole_AfterMigrations_ReadsAndWritesItsTables()
    {
        await BootstrapAsync();
        await using (SampleDbContext migrator = CreateSampleContext(DatabaseRoles.Migrator))
        {
            await migrator.Database.MigrateAsync(TestContext.Current.CancellationToken);
        }

        var item = SampleItem.Create("Stage", 12.3456m);
        item.AddPart("Leg");
        await using (SampleDbContext module = CreateSampleContext("festos_sample"))
        {
            module.SampleItems.Add(item);
            await module.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using SampleDbContext reader = CreateSampleContext("festos_sample");
        SampleItem stored = await reader
            .SampleItems.Include(sample => sample.Parts)
            .SingleAsync(sample => sample.Id == item.Id, TestContext.Current.CancellationToken);
        stored.UnitPrice.ShouldBe(12.3456m);
        stored.Parts.ShouldHaveSingleItem().Label.ShouldBe("Leg");
    }

    private Task BootstrapAsync() =>
        new DatabaseBootstrapper(NullLogger<DatabaseBootstrapper>.Instance).RunAsync(
            database.AdminConnectionString,
            new DatabaseBootstrapOptions(ModuleContexts.Schemas, _passwords),
            TestContext.Current.CancellationToken
        );

    private string ConnectionString(string role) =>
        new NpgsqlConnectionStringBuilder(database.AdminConnectionString)
        {
            Username = role,
            Password = _passwords[role],
            Pooling = false,
        }.ConnectionString;

    private SampleDbContext CreateSampleContext(string role) =>
        ModuleContexts.Create<SampleDbContext>(
            ConnectionString(role),
            SampleModuleDefinition.SchemaName,
            options => new(options)
        );
}
