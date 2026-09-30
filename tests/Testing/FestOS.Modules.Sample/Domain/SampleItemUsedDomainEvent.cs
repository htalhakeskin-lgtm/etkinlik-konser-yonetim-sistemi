using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.Modules.Sample.Domain;

/// <summary>A <see cref="SampleItem"/> was put in use.</summary>
public sealed record SampleItemUsedDomainEvent(SampleItemId SampleItemId) : IDomainEvent;
