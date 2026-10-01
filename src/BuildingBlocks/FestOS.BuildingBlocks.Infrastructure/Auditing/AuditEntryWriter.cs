using System.Diagnostics;
using System.Text.Json;
using FestOS.BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FestOS.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// Save step 4 (building-blocks §6): turns the tracked changes into change history rows, added to the
/// same save so they commit or roll back with the change.
/// </summary>
internal static class AuditEntryWriter
{
    private const string StatusProperty = "status";

    // Already on the history row itself as its time and actor, or bookkeeping that changes every time.
    private static readonly HashSet<string> BookkeepingProperties = new(StringComparer.Ordinal)
    {
        nameof(IAggregateRoot.Version),
        nameof(IAuditable.CreatedAt),
        nameof(IAuditable.CreatedBy),
        nameof(IAuditable.UpdatedAt),
        nameof(IAuditable.UpdatedBy),
    };

    public static void AddEntries(
        DbContext context,
        string module,
        DateTimeOffset occurredAt,
        Guid actorId,
        string actorName
    )
    {
        string? traceId = Activity.Current?.TraceId.ToHexString();
        List<AuditEntry> auditEntries = [];

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (
                entry.Entity is AuditEntry
                || entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)
                || entry.Metadata.ClrType.IsDefined(typeof(NotAuditedAttribute), inherit: true)
                || entry.Metadata.FindAnnotation(ChangeHistoryModelExtensions.NotAuditedAnnotation) is not null
            )
            {
                continue;
            }

            Dictionary<string, Dictionary<string, object?>> changes = Changes(entry);

            // A root whose only change is its version, bumped for a child, has nothing of its own to record.
            if (entry.State == EntityState.Modified && changes.Count == 0)
            {
                continue;
            }

            auditEntries.Add(
                new AuditEntry
                {
                    Id = Guid.CreateVersion7(),
                    OccurredAt = occurredAt,
                    ActorId = actorId,
                    ActorName = actorName,
                    Module = module,
                    EntityType = entry.Metadata.ClrType.Name,
                    EntityId = KeyOf(entry),
                    Action = entry.State switch
                    {
                        EntityState.Added => AuditAction.Created,
                        EntityState.Deleted => AuditAction.Deleted,
                        _ when changes.ContainsKey(StatusProperty) => AuditAction.StatusChanged,
                        _ => AuditAction.Updated,
                    },
                    Changes = JsonSerializer.Serialize(changes),
                    TraceId = traceId,
                }
            );
        }

        context.Set<AuditEntry>().AddRange(auditEntries);
    }

    // New values for a created record, old values for a deleted one, both for each changed field.
    private static Dictionary<string, Dictionary<string, object?>> Changes(EntityEntry entry) =>
        entry
            .Properties.Where(property => IsAudited(property.Metadata))
            .Where(property =>
                entry.State != EntityState.Modified
                || (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
            )
            .ToDictionary(
                property => JsonNamingPolicy.CamelCase.ConvertName(property.Metadata.Name),
                property =>
                    entry.State switch
                    {
                        EntityState.Added => new Dictionary<string, object?>(StringComparer.Ordinal)
                        {
                            ["new"] = Stored(property.Metadata, property.CurrentValue),
                        },
                        EntityState.Deleted => new Dictionary<string, object?>(StringComparer.Ordinal)
                        {
                            ["old"] = Stored(property.Metadata, property.OriginalValue),
                        },
                        _ => new Dictionary<string, object?>(StringComparer.Ordinal)
                        {
                            ["old"] = Stored(property.Metadata, property.OriginalValue),
                            ["new"] = Stored(property.Metadata, property.CurrentValue),
                        },
                    },
                StringComparer.Ordinal
            );

    private static bool IsAudited(IProperty property) =>
        !BookkeepingProperties.Contains(property.Name)
        && property.PropertyInfo?.IsDefined(typeof(NotAuditedAttribute), inherit: true) != true;

    // The value as the database stores it: an identifier's UUID, an enum's camelCase text.
    private static object? Stored(IProperty property, object? value) =>
        value is null
            ? null
            : (property.GetValueConverter() ?? property.GetTypeMapping().Converter)?.ConvertToProvider(value) ?? value;

    private static Guid KeyOf(EntityEntry entry)
    {
        IProperty key = entry.Metadata.FindPrimaryKey()!.Properties.Single();
        return Stored(key, entry.Property(key.Name).CurrentValue) is Guid id
            ? id
            : throw new InvalidOperationException(
                $"{entry.Metadata.DisplayName()} needs a UUID key to appear in the change history."
            );
    }
}
