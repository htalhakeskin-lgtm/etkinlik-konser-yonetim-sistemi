using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>One production with its artist's name and its rider, for its page and its form.</summary>
public sealed record GetProductionQuery(ProductionId Id) : IQuery<ProductionDetails>;
