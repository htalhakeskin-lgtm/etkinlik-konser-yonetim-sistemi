using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>Adds a model to the catalog (US-EQP-002).</summary>
public sealed record CreateEquipmentModelCommand(
    string Brand,
    string Name,
    EquipmentCategoryId CategoryId,
    TrackingType TrackingType,
    ModelMeasures Measures
) : ICommand<EquipmentModelId>, IEquipmentModelDescription;
