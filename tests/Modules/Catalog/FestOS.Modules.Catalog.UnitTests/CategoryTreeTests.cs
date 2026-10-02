using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Domain.Categories;

namespace FestOS.Modules.Catalog.UnitTests;

public sealed class CategoryTreeTests
{
    private static readonly EquipmentCategoryId Sound = EquipmentCategoryId.New();
    private static readonly EquipmentCategoryId Microphone = EquipmentCategoryId.New();
    private static readonly EquipmentCategoryId Vocal = EquipmentCategoryId.New();
    private static readonly EquipmentCategoryId Light = EquipmentCategoryId.New();

    private static readonly CategoryTree Tree = new([
        new CategoryNode(Sound, null, true),
        new CategoryNode(Microphone, Sound, true),
        new CategoryNode(Vocal, Microphone, false),
        new CategoryNode(Light, null, true),
    ]);

    [Fact]
    [Trait("Rule", "BR-EQP-002")]
    public void IsSelfOrBelow_FindsTheCategoryAndEverythingUnderIt()
    {
        Tree.IsSelfOrBelow(Sound, Sound).ShouldBeTrue();
        Tree.IsSelfOrBelow(Vocal, Sound).ShouldBeTrue();
        Tree.IsSelfOrBelow(Light, Sound).ShouldBeFalse();
        Tree.IsSelfOrBelow(Sound, Microphone).ShouldBeFalse();
    }

    [Fact]
    [Trait("Rule", "BR-EQP-002")]
    public void ActiveChildCount_CountsOnlyActiveDirectChildren()
    {
        Tree.ActiveChildCount(Sound).ShouldBe(1);
        Tree.ActiveChildCount(Microphone).ShouldBe(0);
    }

    [Fact]
    public void Create_KeepsTheNameAndItsComparedForm()
    {
        var category = EquipmentCategory.Create("  Işık Masası ", Light);

        category.Name.ShouldBe("Işık Masası");
        category.NameSearch.ShouldBe("isik masasi");
        category.ParentId.ShouldBe(Light);
    }
}
