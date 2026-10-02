using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Catalog.Domain;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.UnitTests;

public sealed class EquipmentModelTests
{
    private static readonly EquipmentCategoryId Microphones = EquipmentCategoryId.New();
    private static readonly ModelMeasures Measures = new(0.298m, null, 0.002m);

    [Fact]
    public void Create_KeepsTheNamesTheirComparedFormAndTheValues()
    {
        var model = EquipmentModel.Create(" Shure ", " SM58 ", Microphones, TrackingType.Serialized, Measures);

        model.DisplayName.ShouldBe("Shure SM58");
        model.BrandNameSearch.ShouldBe("shure sm58");
        model.WeightKilograms.ShouldBe(0.298m);
        model.PowerWatts.ShouldBeNull();
        model.HasStock.ShouldBeFalse();
    }

    [Fact]
    [Trait("Rule", "BR-EQP-001")]
    public void Edit_ChangesTheTrackingTypeOnlyWhileThereIsNoStock()
    {
        var model = EquipmentModel.Create("Klotz", "XLR 10 m", Microphones, TrackingType.Serialized, Measures);
        model.Edit("Klotz", "XLR 10 m", Microphones, TrackingType.Bulk, Measures);
        model.MarkStockCreated();

        model.TrackingType.ShouldBe(TrackingType.Bulk);
        Should
            .Throw<BusinessRuleViolationException>(() =>
                model.Edit("Klotz", "XLR 10 m", Microphones, TrackingType.Serialized, Measures)
            )
            .RuleCode.ShouldBe(CatalogRuleCodes.TrackingTypeFixed);
        model.Edit("Klotz", "XLR 15 m", Microphones, TrackingType.Bulk, Measures);
        model.Name.ShouldBe("XLR 15 m");
    }
}
