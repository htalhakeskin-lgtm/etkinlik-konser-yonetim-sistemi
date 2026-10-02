using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>Changes a production's name and description; its artist stays (riders §5).</summary>
public sealed record EditProductionCommand(ProductionId Id, string Name, string? Description)
    : ICommand<bool>,
        IProductionCommand;
