using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Riders.Domain.Productions;

namespace FestOS.Modules.Riders.Application.Productions;

/// <summary>Takes a production out of new selections (BR-SYS-001).</summary>
public sealed record DeactivateProductionCommand(ProductionId Id) : ICommand<bool>;
