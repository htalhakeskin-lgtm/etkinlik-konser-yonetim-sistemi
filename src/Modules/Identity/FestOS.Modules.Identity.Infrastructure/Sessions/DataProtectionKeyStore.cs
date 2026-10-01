using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>
/// Reads and writes ASP.NET's data protection keys in <c>identity.data_protection_keys</c> (security §7).
/// A plain context, because the key manager saves synchronously, which module contexts refuse; the table
/// itself comes with the Identity migrations.
/// </summary>
internal sealed class DataProtectionKeyStore(DbContextOptions<DataProtectionKeyStore> options)
    : DbContext(options),
        IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder
            .HasDefaultSchema(IdentityModuleDefinition.SchemaName)
            .Entity<DataProtectionKey>()
            .ToTable("data_protection_keys");
}
