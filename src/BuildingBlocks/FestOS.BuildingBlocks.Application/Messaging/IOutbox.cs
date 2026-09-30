using FestOS.BuildingBlocks.Contracts;

namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>
/// Collects the integration events of the current save; domain event handlers add them and the save
/// writes them to the module's outbox in the same transaction (building-blocks §7).
/// </summary>
public interface IOutbox
{
    /// <summary>Adds an event; it is published only if the save commits.</summary>
    void Add(IIntegrationEvent integrationEvent);
}
