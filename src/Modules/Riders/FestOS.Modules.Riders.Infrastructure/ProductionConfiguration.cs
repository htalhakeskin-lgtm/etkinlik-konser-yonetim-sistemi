using FestOS.Modules.Riders.Domain.Productions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Riders.Infrastructure;

internal sealed class ProductionConfiguration : IEntityTypeConfiguration<Production>
{
    public void Configure(EntityTypeBuilder<Production> builder)
    {
        builder.ToTable("productions");
        builder.Property(production => production.Name).HasMaxLength(Production.NameMaxLength);
        builder.Property(production => production.NameSearch).HasMaxLength(Production.NameMaxLength);
        builder.Property(production => production.Description).HasMaxLength(Production.DescriptionMaxLength);

        // One production per name for an artist, deactivated ones included (BR-RDR-009). The artist is a
        // party, another module's record; this index, led by it, also serves its lookups (DT-03).
        builder
            .HasIndex(production => new { production.ArtistPartyId, production.NameSearch })
            .IsUnique()
            .HasDatabaseName("ux_productions_artist_name");
    }
}
