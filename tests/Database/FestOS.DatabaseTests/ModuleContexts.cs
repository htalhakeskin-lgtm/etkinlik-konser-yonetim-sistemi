using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Sample.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FestOS.DatabaseTests;

/// <summary>
/// Every module's database context, for the migration and model tests. The sample module stands in
/// until the first business module arrives; each new module is added here.
/// </summary>
internal static class ModuleContexts
{
    /// <summary>A connection string that cannot reach a server, for tests that only read the model.</summary>
    public const string ModelOnlyConnectionString = "Host=design-time.invalid;Database=festos";

    public static IReadOnlyList<string> Schemas { get; } = [SampleModuleDefinition.SchemaName];

    public static IEnumerable<ModuleDbContext> Create(string connectionString) =>
        [Create<SampleDbContext>(connectionString, SampleModuleDefinition.SchemaName, options => new(options))];

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
