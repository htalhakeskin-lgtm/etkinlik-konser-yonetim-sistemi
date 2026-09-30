namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// A module's database as the migrate command sees it: its schema and how to apply its migrations
/// with a given connection string. Registered by <c>AddModuleDbContext</c>.
/// </summary>
/// <param name="Schema">The module's schema.</param>
/// <param name="Migrate">Applies the module's pending migrations with the given connection string.</param>
public sealed record ModuleDatabase(string Schema, Func<string, CancellationToken, Task> Migrate);
