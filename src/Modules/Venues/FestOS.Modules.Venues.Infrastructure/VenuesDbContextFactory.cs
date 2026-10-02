using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FestOS.Modules.Venues.Infrastructure;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model. The host name cannot resolve, so a tool
/// command that tries to connect fails instead of reaching whatever database runs locally.
/// </summary>
internal sealed class VenuesDbContextFactory : IDesignTimeDbContextFactory<VenuesDbContext>
{
    public VenuesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<VenuesDbContext>();
        options.UseModuleDatabase("Host=design-time.invalid;Database=festos", VenuesModuleDefinition.SchemaName);
        return new VenuesDbContext(options.Options);
    }
}
