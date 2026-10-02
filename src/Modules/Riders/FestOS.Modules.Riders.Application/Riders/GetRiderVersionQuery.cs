using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>One version with its lines named through Catalog, for the rider view, the form and the comparison.</summary>
public sealed record GetRiderVersionQuery(RiderVersionId Id) : IQuery<RiderVersionDetails>;
