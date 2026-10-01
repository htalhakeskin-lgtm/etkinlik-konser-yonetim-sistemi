using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Audit.Infrastructure;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FestOS.DatabaseTests;

/// <summary>
/// Every module's database context, for the migration and model tests; each new module is added
/// here. The sample module stands in for business modules until the first one arrives.
/// </summary>
internal static class ModuleContexts
{
    /// <summary>A connection string that cannot reach a server, for tests that only read the model.</summary>
    public const string ModelOnlyConnectionString = "Host=design-time.invalid;Database=festos";

    public static IReadOnlyList<string> Schemas { get; } =
    [AuditModuleDefinition.SchemaName, IdentityModuleDefinition.SchemaName, SampleModuleDefinition.SchemaName];

    /// <summary>New passwords for the migrator and every module role, for a bootstrap.</summary>
    public static Dictionary<string, string> NewPasswords() =>
        new[] { DatabaseRoles.Migrator }
            .Concat(Schemas.Select(DatabaseRoles.ForModule))
            .ToDictionary(role => role, _ => Guid.CreateVersion7().ToString("N"), StringComparer.Ordinal);

    public static IEnumerable<ModuleDbContext> Create(string connectionString) =>
        [
            Create<AuditDbContext>(connectionString, AuditModuleDefinition.SchemaName, options => new(options)),
            Create<IdentityDbContext>(connectionString, IdentityModuleDefinition.SchemaName, options => new(options)),
            Create<SampleDbContext>(connectionString, SampleModuleDefinition.SchemaName, options => new(options)),
        ];

    public static TContext Create<TContext>(
        string connectionString,
        string schema,
        Func<DbContextOptions<TContext>, TContext> create
    )
        where TContext : ModuleDbContext
    {
        var options = new DbContextOptionsBuilder<TContext>();
        options.UseModuleDatabase(connectionString, schema);
        return create(options.Options);
    }
}
