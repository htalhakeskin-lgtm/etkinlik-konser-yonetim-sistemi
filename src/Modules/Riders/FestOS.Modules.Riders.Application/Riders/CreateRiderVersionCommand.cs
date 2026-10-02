using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>Saves a rider's lines as its next version (US-RDR-001, US-RDR-002).</summary>
public sealed record CreateRiderVersionCommand(RiderId RiderId, IReadOnlyList<RiderLineDetails> Lines, string? Note)
    : ICommand<RiderVersionId>;
