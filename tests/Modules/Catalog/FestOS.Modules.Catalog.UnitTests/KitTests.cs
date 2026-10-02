using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Catalog.Domain;
using FestOS.Modules.Catalog.Domain.Kits;
using FestOS.Modules.Catalog.Domain.Models;

namespace FestOS.Modules.Catalog.UnitTests;

public sealed class KitTests
{
    private static readonly EquipmentModelId Spot = EquipmentModelId.New();
    private static readonly EquipmentModelId Cable = EquipmentModelId.New();

    [Fact]
    public void Edit_KeepsTheLineOfTheSameTarget_AndFollowsTheNewOrder()
    {
        var kit = Kit.Create(" Küçük sahne ışık paketi ", [new(Spot, null, 4), new(Cable, null, 8)]);
        KitLine spotLine = kit.Lines[0];

        kit.Edit("Küçük sahne ışık paketi", [new(Cable, null, 10), new(Spot, null, 6)]);

        kit.Name.ShouldBe("Küçük sahne ışık paketi");
        kit.Lines[1].ShouldBeSameAs(spotLine);
        spotLine.Quantity.ShouldBe(6);
        kit.Lines[0].Quantity.ShouldBe(10);
    }

    [Fact]
    [Trait("Rule", "BR-EQP-003")]
    public void Edit_RefusesAKitThatListsItself()
    {
        var kit = Kit.Create("Paket", []);

        Should
            .Throw<BusinessRuleViolationException>(() => kit.Edit("Paket", [new(null, kit.Id, 1)]))
            .RuleCode.ShouldBe(CatalogRuleCodes.KitStructure);
    }
}
