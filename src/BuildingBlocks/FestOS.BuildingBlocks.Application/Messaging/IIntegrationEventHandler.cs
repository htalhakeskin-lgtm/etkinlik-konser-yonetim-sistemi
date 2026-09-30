using System.Diagnostics.CodeAnalysis;
using FestOS.BuildingBlocks.Contracts;

namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>
/// Reacts to another module's integration event in the listener's own module: its own transaction, and
/// at most once per event thanks to the inbox (05 §9.2). Named after its effect, e.g.
/// <c>ReleaseReservationsOnEventCancelledHandler</c> (naming §4.2).
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "The name is the documented building block (08 §4); it is not a .NET event delegate."
)]
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    /// <summary>Handles the event.</summary>
    Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
