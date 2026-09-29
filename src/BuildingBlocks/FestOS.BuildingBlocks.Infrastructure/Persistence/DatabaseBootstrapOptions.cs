namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>What the database bootstrap sets up.</summary>
/// <param name="ModuleSchemas">The schema of every module; each gets a <c>festos_{schema}</c> role.</param>
/// <param name="Passwords">
/// Passwords by role name, from the secret store (security §6). The migrator and every module role
/// need one; <see cref="DatabaseRoles.ReadOnly"/> and <see cref="DatabaseRoles.Monitor"/> cannot log
/// in until they get one.
/// </param>
public sealed record DatabaseBootstrapOptions(
    IReadOnlyCollection<string> ModuleSchemas,
    IReadOnlyDictionary<string, string> Passwords
);
