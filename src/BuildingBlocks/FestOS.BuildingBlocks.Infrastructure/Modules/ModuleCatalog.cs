namespace FestOS.BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// The registered modules, in registration order. Used to map endpoints, to list the schemas for the
/// database bootstrap and migrations, and to build the permission catalog for Identity.
/// </summary>
public sealed class ModuleCatalog(IReadOnlyList<IModuleDefinition> modules)
{
    /// <summary>The modules, in the order the Host listed them.</summary>
    public IReadOnlyList<IModuleDefinition> Modules { get; } = modules;

    /// <summary>Every module's permissions together.</summary>
    public IReadOnlyCollection<string> Permissions { get; } =
    [.. modules.SelectMany(module => module.Permissions).Order(StringComparer.Ordinal)];
}
