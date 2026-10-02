using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>Records an artist's production and opens its empty rider (US-ART-001, riders RD-01).</summary>
public sealed record CreateProductionCommand(Guid ArtistPartyId, string Name, string? Description)
    : ICommand<ProductionId>,
        IProductionCommand;
