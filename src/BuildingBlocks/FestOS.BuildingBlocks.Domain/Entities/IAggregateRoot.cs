using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.BuildingBlocks.Domain.Entities;

/// <summary>
/// What the unit of work needs from any aggregate root, whatever its identifier type: the concurrency
/// version and the domain events waiting to be handled.
/// </summary>
public interface IAggregateRoot : IAuditable
{
    /// <summary>The concurrency version (database §11.1).</summary>
    int Version { get; }

    /// <summary>Domain events raised since the aggregate was loaded or last saved.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Returns the raised domain events and clears them.</summary>
    IReadOnlyList<IDomainEvent> DequeueDomainEvents();
}
