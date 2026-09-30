using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Sample.Domain;
using FestOS.Modules.Sample.Infrastructure;
using FestOS.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FestOS.DatabaseTests;

/// <summary>The migrate command end to end, with the Host's own registration (building-blocks §5.4).</summary>
public sealed class DatabaseMigratorTests(PostgresDatabase database)
{
    [Fact]
    [Trait("DatabaseRule", "DT-01")]
    public async Task Run_ForTheRegisteredModules_PreparesTheDatabaseAndLetsModulesConnect()
    {
        using IHost host = BuildHost();

        await host.Services.GetRequiredService<DatabaseMigrator>().RunAsync(TestContext.Current.CancellationToken);

        var item = SampleItem.Create("Stage", 1m);
        await using (AsyncServiceScope scope = host.Services.CreateAsyncScope())
        {
            SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            context.SampleItems.Add(item);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (AsyncServiceScope scope = host.Services.CreateAsyncScope())
        {
            SampleDbContext context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            (
                await context.SampleItems.AnyAsync(
                    sample => sample.Id == item.Id,
                    TestContext.Current.CancellationToken
                )
            ).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Run_TwiceInARow_Succeeds()
    {
        using IHost host = BuildHost();
        DatabaseMigrator migrator = host.Services.GetRequiredService<DatabaseMigrator>();

        await migrator.RunAsync(TestContext.Current.CancellationToken);
        await migrator.RunAsync(TestContext.Current.CancellationToken);
    }

    // Registers the sample module exactly as the Host registers its modules.
    private IHost BuildHost()
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [$"ConnectionStrings:{DatabaseConnections.ConnectionStringName}"] = database.AdminConnectionString,
                [$"Database:Passwords:{DatabaseRoles.Migrator}"] = Guid.CreateVersion7().ToString("N"),
                ["Database:Passwords:festos_audit"] = Guid.CreateVersion7().ToString("N"),
                ["Database:Passwords:festos_sample"] = Guid.CreateVersion7().ToString("N"),
            }
        );
        builder.Services.AddLogging();
        builder.Services.AddSingleton<ICurrentUser>(new FakeCurrentUser());
        builder.AddModules(new AuditModuleDefinition(), new SampleModuleDefinition());
        return builder.Build();
    }
}
