using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Identity.Domain;
using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure;

/// <summary>The Identity module's context, on the <c>identity</c> schema.</summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : ModuleDbContext(options, IdentityModuleDefinition.SchemaName)
{
    /// <summary>The users.</summary>
    public DbSet<User> Users => Set<User>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal) { ["ux_users_email"] = IdentityRuleCodes.UniqueEmail };
}
