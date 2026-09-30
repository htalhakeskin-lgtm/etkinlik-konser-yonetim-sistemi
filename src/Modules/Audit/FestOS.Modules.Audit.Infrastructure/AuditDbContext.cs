using FestOS.BuildingBlocks.Infrastructure.Auditing;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Audit.Infrastructure;

/// <summary>
/// The Audit module's context. It owns <c>audit.audit_entries</c>, which the shared model maps in every
/// module's context; here it is part of the migrations.
/// </summary>
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options)
    : ModuleDbContext(options, AuditModuleDefinition.SchemaName)
{
    /// <summary>The change history.</summary>
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
}
