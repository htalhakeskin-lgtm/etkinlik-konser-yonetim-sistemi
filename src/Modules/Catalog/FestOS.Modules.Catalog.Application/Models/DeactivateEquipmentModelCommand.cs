using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>Takes a model out of new selections (BR-SYS-001).</summary>
public sealed record DeactivateEquipmentModelCommand(EquipmentModelId Id) : ICommand<bool>;
