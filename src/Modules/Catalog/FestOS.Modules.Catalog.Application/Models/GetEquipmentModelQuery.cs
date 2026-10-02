using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>One model, for its page and its form.</summary>
public sealed record GetEquipmentModelQuery(EquipmentModelId Id) : IQuery<EquipmentModelDetails>;
