namespace FestOS.BuildingBlocks.Domain.Identifiers;

/// <summary>
/// A module's own identifier type, so an <c>EventId</c> cannot be passed where a <c>VenueId</c> is
/// expected (database §5.2). One EF and one JSON converter handle every such type through
/// <see cref="From"/>; at module boundaries identifiers are plain <see cref="Guid"/>s.
/// </summary>
/// <example>
/// <code>
/// public readonly record struct EventId(Guid Value) : IStronglyTypedId&lt;EventId&gt;
/// {
///     public static EventId From(Guid value) => new(value);
///
///     public static EventId New() => new(Guid.CreateVersion7());
/// }
/// </code>
/// </example>
/// <typeparam name="TSelf">The implementing identifier type.</typeparam>
public interface IStronglyTypedId<TSelf>
    where TSelf : struct, IStronglyTypedId<TSelf>
{
    /// <summary>The underlying UUID (version 7, database §5.1).</summary>
    Guid Value { get; }

    /// <summary>Wraps an existing UUID, e.g. one read from the database or a request.</summary>
    static abstract TSelf From(Guid value);
}
