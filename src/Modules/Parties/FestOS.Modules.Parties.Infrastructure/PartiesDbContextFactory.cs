using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FestOS.Modules.Parties.Infrastructure;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model. The host name cannot resolve, so a tool
/// command that tries to connect fails instead of reaching whatever database runs locally.
/// </summary>
internal sealed class PartiesDbContextFactory : IDesignTimeDbContextFactory<PartiesDbContext>
{
    public PartiesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PartiesDbContext>();
        options.UseModuleDatabase("Host=design-time.invalid;Database=festos", PartiesModuleDefinition.SchemaName);
        return new PartiesDbContext(options.Options);
    }
}
