using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>Opens a deactivated production again.</summary>
public sealed record ActivateProductionCommand(ProductionId Id) : ICommand<bool>;
