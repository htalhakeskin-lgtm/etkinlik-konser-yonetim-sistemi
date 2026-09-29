namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>An object whose identity stays the same over its lifetime.</summary>
/// <typeparam name="TId">The identifier type, usually a strongly typed id (database §5.2).</typeparam>
public abstract class Entity<TId>
    where TId : struct
{
    /// <summary>Creates the entity with an identifier chosen by the domain (database §5.1).</summary>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>The identifier; assigned once and never changed.</summary>
    public TId Id { get; private init; }
}
