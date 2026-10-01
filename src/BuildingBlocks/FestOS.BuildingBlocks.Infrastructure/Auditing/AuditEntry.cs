namespace FestOS.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// One row of the change history, <c>audit.audit_entries</c> (database §14.2). Every module writes its
/// own rows in the same transaction as the change; the Audit module owns the table.
/// </summary>
public sealed class AuditEntry
{
    /// <summary>The Audit module's schema.</summary>
    public const string SchemaName = "audit";

    /// <summary>The table name.</summary>
    public const string TableName = "audit_entries";

    /// <summary>The entry's identifier (UUIDv7).</summary>
    public Guid Id { get; init; }

    /// <summary>When the change was saved (UTC).</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>The user who made the change, or the system user.</summary>
    public Guid ActorId { get; init; }

    /// <summary>The user's name when the change was made; reading never asks Identity (audit AU-01).</summary>
    public required string ActorName { get; init; }

    /// <summary>The schema of the module that owns the record, e.g. <c>booking</c>.</summary>
    public required string Module { get; init; }

    /// <summary>The kind of record, e.g. <c>VenueHold</c>.</summary>
    public required string EntityType { get; init; }

    /// <summary>The record's identifier.</summary>
    public Guid EntityId { get; init; }

    /// <summary>What happened.</summary>
    public AuditAction Action { get; init; }

    /// <summary>Each changed field's old and new value, as JSON.</summary>
    public required string Changes { get; init; }

    /// <summary>The OpenTelemetry trace of the request that made the change.</summary>
    public string? TraceId { get; init; }
}
