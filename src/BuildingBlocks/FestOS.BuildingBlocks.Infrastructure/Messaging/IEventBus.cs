using FestOS.BuildingBlocks.Contracts;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Delivers an integration event to every listener. In process for now; with more than one application
/// instance a PostgreSQL LISTEN/NOTIFY or queue implementation can take its place (ADR-0010).
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Delivers the event to all listeners in parallel; throws an <see cref="AggregateException"/> with
    /// every listener's failure after all of them finished.
    /// </summary>
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
