using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>Opens a deactivated model again, in an active category (BR-EQP-002).</summary>
public sealed record ActivateEquipmentModelCommand(EquipmentModelId Id) : ICommand<bool>;
