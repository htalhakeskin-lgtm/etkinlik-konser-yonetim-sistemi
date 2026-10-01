using FestOS.BuildingBlocks.Infrastructure.Auditing;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

internal sealed class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    // ASP.NET's own records: a key is a secret, never history.
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder) =>
        builder.ToTable("data_protection_keys").ExcludeFromChangeHistory();
}
