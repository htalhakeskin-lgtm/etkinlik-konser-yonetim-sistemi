using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Inventory.Domain.Warehouses;
using FestOS.Modules.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.Modules.Inventory.IntegrationTests;

public sealed class WarehousePersistenceTests(InventoryFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Warehouse_IsStoredWithItsAddress()
    {
        var warehouse = Warehouse.Create("Merkez Depo", "İstanbul", "Ataşehir, Depo Sk. 4");
        await SaveAsync(warehouse);

        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        Warehouse stored = await scope
            .ServiceProvider.GetRequiredService<InventoryDbContext>()
            .Warehouses.SingleAsync(found => found.Id == warehouse.Id, Cancellation);
        stored.Name.ShouldBe("Merkez Depo");
        stored.Address.ShouldBe("Ataşehir, Depo Sk. 4");
        stored.Version.ShouldBe(1);
    }

    [Fact]
    [Trait("Rule", "BR-SYS-016")]
    public async Task SecondWarehouseWithTheSameName_IsRefusedWhateverTheCaseAndMarks()
    {
        await SaveAsync(Warehouse.Create("Işıklar Depo", "İzmir", "Adres 1"));

        BusinessRuleViolationException refused = await Should.ThrowAsync<BusinessRuleViolationException>(() =>
            SaveAsync(Warehouse.Create("ISIKLAR DEPO", "Ankara", "Adres 2"))
        );

        refused.RuleCode.ShouldBe("BR-SYS-016");
    }

    private async Task SaveAsync(Warehouse warehouse)
    {
        await using AsyncServiceScope scope = fixture.Services.CreateAsyncScope();
        InventoryDbContext context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync(Cancellation);
    }
}
