using Microsoft.EntityFrameworkCore;

namespace FestOS.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// Maps <see cref="AuditEntry"/> in every module's context. Only the Audit module's migrations create
/// the table; the other modules only insert into it (05 §5.2).
/// </summary>
internal static class AuditEntryModel
{
    public static void Configure(ModelBuilder modelBuilder, bool ownsTable) =>
        modelBuilder.Entity<AuditEntry>(entry =>
        {
            entry.ToTable(
                AuditEntry.TableName,
                AuditEntry.SchemaName,
                table =>
                {
                    if (!ownsTable)
                    {
                        table.ExcludeFromMigrations();
                    }
                }
            );
            entry.Property(auditEntry => auditEntry.Module).HasMaxLength(64);
            entry.Property(auditEntry => auditEntry.EntityType).HasMaxLength(200);
            entry.Property(auditEntry => auditEntry.Changes).HasColumnType("jsonb");
            entry.Property(auditEntry => auditEntry.TraceId).HasMaxLength(32);

            // A record's history is read by this index (database §14.2).
            entry.HasIndex(auditEntry => new
            {
                auditEntry.EntityType,
                auditEntry.EntityId,
                auditEntry.OccurredAt,
            });
        });
}
