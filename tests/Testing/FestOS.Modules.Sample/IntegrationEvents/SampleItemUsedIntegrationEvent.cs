using FestOS.BuildingBlocks.Contracts;

namespace FestOS.Modules.Sample.IntegrationEvents;

/// <summary>Tells other modules that an item was put in use; ordered per item.</summary>
public sealed record SampleItemUsedIntegrationEvent : IntegrationEvent
{
    /// <summary>The item.</summary>
    public required Guid SampleItemId { get; init; }
}
