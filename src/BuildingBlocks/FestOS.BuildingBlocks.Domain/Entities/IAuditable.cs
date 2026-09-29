namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>
/// Who created and last changed a record, and when (database §9). Filled in by the unit of work from
/// the current user and <see cref="TimeProvider"/>; background work uses the system user.
/// </summary>
public interface IAuditable
{
    /// <summary>When the record was created (UTC).</summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>The user who created the record.</summary>
    Guid CreatedBy { get; }

    /// <summary>When the record was last changed (UTC).</summary>
    DateTimeOffset UpdatedAt { get; }

    /// <summary>The user who last changed the record.</summary>
    Guid UpdatedBy { get; }
}
