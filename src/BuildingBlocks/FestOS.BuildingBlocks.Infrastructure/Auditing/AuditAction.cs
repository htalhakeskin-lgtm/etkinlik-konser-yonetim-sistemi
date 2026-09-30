namespace FestOS.BuildingBlocks.Infrastructure.Auditing;

/// <summary>What happened to a record (database §14.2); stored as camelCase text.</summary>
public enum AuditAction
{
    /// <summary>The record was created.</summary>
    Created,

    /// <summary>One or more fields changed.</summary>
    Updated,

    /// <summary>The record was deleted.</summary>
    Deleted,

    /// <summary>The status field changed, possibly with other fields.</summary>
    StatusChanged,
}
