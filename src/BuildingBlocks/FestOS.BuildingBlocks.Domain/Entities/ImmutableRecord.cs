namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>The base of a record that is only ever added (database §14.1); see <see cref="IImmutableRecord"/>.</summary>
public abstract class ImmutableRecord<TId> : Entity<TId>, IImmutableRecord
    where TId : struct
{
    /// <inheritdoc />
    protected ImmutableRecord(TId id)
        : base(id) { }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public Guid CreatedBy { get; private set; }
}
