using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Riders;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>A rider's versions, newest first, with the rider's version for the next save (US-RDR-002).</summary>
public sealed record ListRiderVersionsQuery(RiderId RiderId) : IQuery<RiderVersionList>;
