using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FestOS.Modules.Inventory.Infrastructure;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model. The host name cannot resolve, so a tool
/// command that tries to connect fails instead of reaching whatever database runs locally.
/// </summary>
internal sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>();
        options.UseModuleDatabase("Host=design-time.invalid;Database=festos", InventoryModuleDefinition.SchemaName);
        return new InventoryDbContext(options.Options);
    }
}
