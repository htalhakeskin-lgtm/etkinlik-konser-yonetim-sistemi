using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>
/// The root of an aggregate: the only entity other code changes, the boundary of a transaction and
/// the unit of optimistic concurrency.
/// </summary>
/// <remarks>
/// <see cref="Version"/> and the audit fields are written by the unit of work, never by domain code
/// (database §9, §11.1).
/// </remarks>
public abstract class AggregateRoot<TId> : Entity<TId>, IAuditable
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc />
    protected AggregateRoot(TId id)
        : base(id) { }

    /// <summary>
    /// Concurrency version. The unit of work increments it whenever the root or any entity inside
    /// the aggregate changes; a stale version is rejected (BR-SYS-011).
    /// </summary>
    public int Version { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public Guid CreatedBy { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <inheritdoc />
    public Guid UpdatedBy { get; private set; }

    /// <summary>Domain events raised since the aggregate was loaded or last saved.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Returns the raised domain events and clears them; called by the unit of work.</summary>
    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        IDomainEvent[] domainEvents = [.. _domainEvents];
        _domainEvents.Clear();
        return domainEvents;
    }

    /// <summary>Records a domain event; it is handled in the same transaction when the change is saved.</summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
