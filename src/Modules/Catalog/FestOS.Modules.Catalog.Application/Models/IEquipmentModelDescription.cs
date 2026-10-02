using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>What creating and editing a model both send, so one set of checks covers both (catalog §6).</summary>
public interface IEquipmentModelDescription
{
    /// <summary>The brand.</summary>
    string Brand { get; }

    /// <summary>The model name.</summary>
    string Name { get; }

    /// <summary>The category.</summary>
    EquipmentCategoryId CategoryId { get; }

    /// <summary>Serial-numbered or counted.</summary>
    TrackingType TrackingType { get; }

    /// <summary>The optional technical values.</summary>
    ModelMeasures Measures { get; }
}
