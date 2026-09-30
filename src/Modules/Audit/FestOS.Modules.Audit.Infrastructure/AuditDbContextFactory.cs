using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FestOS.Modules.Audit.Infrastructure;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model. The host name cannot resolve, so a tool
/// command that tries to connect fails instead of reaching whatever database runs locally.
/// </summary>
internal sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>();
        options.UseModuleDatabase("Host=design-time.invalid;Database=festos", AuditModuleDefinition.SchemaName);
        return new AuditDbContext(options.Options);
    }
}
