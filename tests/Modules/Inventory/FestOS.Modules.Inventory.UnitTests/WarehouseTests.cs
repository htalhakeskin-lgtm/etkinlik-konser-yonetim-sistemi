using FestOS.Modules.Inventory.Domain.Warehouses;

namespace FestOS.Modules.Inventory.UnitTests;

public sealed class WarehouseTests
{
    [Fact]
    [Trait("Rule", "BR-SYS-016")]
    public void Create_KeepsTheNameAndItsComparedForm()
    {
        var warehouse = Warehouse.Create("  Işıklar Depo ", " İzmir ", " Liman Cad. 1 ");

        warehouse.Name.ShouldBe("Işıklar Depo");
        warehouse.NameSearch.ShouldBe("isiklar depo");
        warehouse.City.ShouldBe("İzmir");
        warehouse.Address.ShouldBe("Liman Cad. 1");
    }

    [Fact]
    public void Edit_DescribesTheWarehouseAgain()
    {
        var warehouse = Warehouse.Create("Merkez Depo", "İstanbul", "Adres 1");

        warehouse.Edit("Kuzey Depo", "Ankara", "Adres 2");

        warehouse.Name.ShouldBe("Kuzey Depo");
        warehouse.NameSearch.ShouldBe("kuzey depo");
        warehouse.City.ShouldBe("Ankara");
    }
}
