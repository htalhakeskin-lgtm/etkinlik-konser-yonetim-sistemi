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

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public void Deactivate_KeepsTheWarehouseAndTellsOnce_AndActivateOpensItAgain()
    {
        var warehouse = Warehouse.Create("Merkez Depo", "İstanbul", "Adres 1");
        var by = Guid.CreateVersion7();
        var at = new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);

        warehouse.Deactivate(by, at);
        warehouse.Deactivate(Guid.CreateVersion7(), at.AddDays(1));
        DateTimeOffset? deactivatedAt = warehouse.DeactivatedAt;
        warehouse.Activate();

        deactivatedAt.ShouldBe(at);
        warehouse.DequeueDomainEvents().ShouldBe([new WarehouseDeactivatedDomainEvent(warehouse.Id)]);
        warehouse.DeactivatedAt.ShouldBeNull();
    }
}
