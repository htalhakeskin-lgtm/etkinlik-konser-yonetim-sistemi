using System.Diagnostics.CodeAnalysis;
using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>
/// Reacts to a domain event inside the same module and transaction, while the change is being saved
/// (building-blocks §4). Several handlers may handle the same event.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "The name is the documented building block (08 §4); it is not a .NET event delegate."
)]
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    /// <summary>Handles the domain event.</summary>
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
