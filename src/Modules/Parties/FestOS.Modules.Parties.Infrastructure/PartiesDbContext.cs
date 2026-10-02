using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Parties.Domain;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Parties.Infrastructure;

/// <summary>The Parties module's context, on the <c>parties</c> schema.</summary>
public sealed class PartiesDbContext(DbContextOptions<PartiesDbContext> options)
    : ModuleDbContext(options, PartiesModuleDefinition.SchemaName)
{
    /// <summary>The parties.</summary>
    public DbSet<Party> Parties => Set<Party>();

    /// <inheritdoc />
    protected override IReadOnlyDictionary<string, string> ConstraintRules { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ck_parties_kind_names"] = PartiesRuleCodes.PartyRoles,
            ["ck_parties_roles_present"] = PartiesRuleCodes.PartyRoles,
        };
}
