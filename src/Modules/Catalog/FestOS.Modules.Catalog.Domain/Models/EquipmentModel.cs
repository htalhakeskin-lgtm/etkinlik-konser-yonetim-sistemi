using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.BuildingBlocks.Domain.Text;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.Domain.Models;

/// <summary>
/// A make of equipment, e.g. Shure SM58 (US-EQP-002). Stock, riders and venue equipment refer to it, so it is
/// never deleted, only deactivated (BR-SYS-001).
/// </summary>
public sealed class EquipmentModel : AggregateRoot<EquipmentModelId>, IDeactivatable
{
    /// <summary>The longest brand.</summary>
    public const int BrandMaxLength = 100;

    /// <summary>The longest model name.</summary>
    public const int NameMaxLength = 200;

    private EquipmentModel(EquipmentModelId id)
        : base(id) { }

    /// <summary>The brand, e.g. Shure.</summary>
    public string Brand { get; private set; } = string.Empty;

    /// <summary>The model name, e.g. SM58.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The search key of the brand and name together: what makes two models the same (BR-EQP-012).</summary>
    [NotAudited]
    public string BrandNameSearch { get; private set; } = string.Empty;

    /// <summary>The category it belongs to.</summary>
    public EquipmentCategoryId CategoryId { get; private set; }

    /// <summary>Serial-numbered or counted.</summary>
    public TrackingType TrackingType { get; private set; }

    /// <summary>Weight in kilograms, if known.</summary>
    public decimal? WeightKilograms { get; private set; }

    /// <summary>Power draw in watts, if known.</summary>
    public int? PowerWatts { get; private set; }

    /// <summary>Volume when packed, in cubic meters, if known.</summary>
    public decimal? TransportVolumeCubicMeters { get; private set; }

    /// <summary>Whether a unit or a counted stock of it exists; set from Inventory (1.5) and never cleared.</summary>
    public bool HasStock { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>The name lists show: the brand and the model name.</summary>
    public string DisplayName => $"{Brand} {Name}";

    /// <summary>A new, active model; the command checks that the category is active.</summary>
    public static EquipmentModel Create(
        string brand,
        string name,
        EquipmentCategoryId categoryId,
        TrackingType trackingType,
        ModelMeasures measures
    )
    {
        var model = new EquipmentModel(EquipmentModelId.New()) { TrackingType = trackingType };
        model.Describe(brand, name, categoryId, measures);
        return model;
    }

    /// <summary>Changes the model; the tracking type only while it has no stock (BR-EQP-001).</summary>
    public void Edit(
        string brand,
        string name,
        EquipmentCategoryId categoryId,
        TrackingType trackingType,
        ModelMeasures measures
    )
    {
        if (HasStock && trackingType != TrackingType)
        {
            throw new BusinessRuleViolationException(
                CatalogRuleCodes.TrackingTypeFixed,
                "A model with stock keeps its tracking type."
            );
        }

        TrackingType = trackingType;
        Describe(brand, name, categoryId, measures);
    }

    /// <summary>Records that stock of the model exists, which fixes its tracking type (BR-EQP-001).</summary>
    public void MarkStockCreated() => HasStock = true;

    /// <summary>Takes the model out of new selections (BR-SYS-001).</summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
        }
    }

    /// <summary>Opens a deactivated model again; the command checks that its category is active.</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }

    private void Describe(string brand, string name, EquipmentCategoryId categoryId, ModelMeasures measures)
    {
        Brand = brand.Trim();
        Name = name.Trim();
        BrandNameSearch = SearchKey.Of($"{Brand} {Name}");
        CategoryId = categoryId;
        WeightKilograms = measures.WeightKilograms;
        PowerWatts = measures.PowerWatts;
        TransportVolumeCubicMeters = measures.TransportVolumeCubicMeters;
    }
}
