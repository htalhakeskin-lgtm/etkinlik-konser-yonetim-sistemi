using FestOS.BuildingBlocks.Contracts;

namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>How one integration event type becomes a resource change; registered by the module that owns it.</summary>
internal sealed record ResourceChangeMapping(Type EventType, Func<IIntegrationEvent, ResourceChange> Map);
