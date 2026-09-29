namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>
/// Master data that other modules may reference is deactivated instead of deleted (BR-SYS-001,
/// database §10.1). A deactivated record stays visible where it is already used.
/// </summary>
public interface IDeactivatable
{
    /// <summary>When the record was deactivated (UTC); <see langword="null"/> while it is active.</summary>
    DateTimeOffset? DeactivatedAt { get; }

    /// <summary>The user who deactivated the record.</summary>
    Guid? DeactivatedBy { get; }

    /// <summary>Whether the record is active.</summary>
    bool IsActive => DeactivatedAt is null;
}
