using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FestOS.Modules.Riders.Infrastructure;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model. The host name cannot resolve, so a tool
/// command that tries to connect fails instead of reaching whatever database runs locally.
/// </summary>
internal sealed class RidersDbContextFactory : IDesignTimeDbContextFactory<RidersDbContext>
{
    public RidersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RidersDbContext>();
        options.UseModuleDatabase("Host=design-time.invalid;Database=festos", RidersModuleDefinition.SchemaName);
        return new RidersDbContext(options.Options);
    }
}
