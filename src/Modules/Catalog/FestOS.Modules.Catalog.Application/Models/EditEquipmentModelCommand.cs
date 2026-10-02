using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>Changes a model; the tracking type only while it has no stock (BR-EQP-001).</summary>
public sealed record EditEquipmentModelCommand(
    EquipmentModelId Id,
    string Brand,
    string Name,
    EquipmentCategoryId CategoryId,
    TrackingType TrackingType,
    ModelMeasures Measures
) : ICommand<bool>, IEquipmentModelDescription;
