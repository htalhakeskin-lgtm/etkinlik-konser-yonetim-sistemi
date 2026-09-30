using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.Modules.Sample.Domain;

/// <summary>Raised again by its own handler, to test the limit on domain event rounds.</summary>
public sealed record SampleItemEchoedDomainEvent(SampleItemId SampleItemId) : IDomainEvent;
