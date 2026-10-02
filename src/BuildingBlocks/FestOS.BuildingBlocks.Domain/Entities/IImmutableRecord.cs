namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>
/// A record that is only ever added (database §9, §14.1): who created it and when, with no version and no
/// "last changed" fields. The unit of work fills them in and refuses to change or delete the record.
/// </summary>
public interface IImmutableRecord
{
    /// <summary>When the record was created (UTC).</summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>The user who created the record.</summary>
    Guid CreatedBy { get; }
}
