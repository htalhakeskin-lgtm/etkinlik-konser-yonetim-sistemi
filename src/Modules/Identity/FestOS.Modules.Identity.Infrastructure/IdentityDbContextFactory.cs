using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FestOS.Modules.Identity.Infrastructure;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model. The host name cannot resolve, so a tool
/// command that tries to connect fails instead of reaching whatever database runs locally.
/// </summary>
internal sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>();
        options.UseModuleDatabase("Host=design-time.invalid;Database=festos", IdentityModuleDefinition.SchemaName);
        return new IdentityDbContext(options.Options);
    }
}
