using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Parties.Infrastructure;

internal sealed class ArtistRepresentationConfiguration : IEntityTypeConfiguration<ArtistRepresentation>
{
    public void Configure(EntityTypeBuilder<ArtistRepresentation> builder)
    {
        builder.ToTable("artist_representations");
        builder
            .Property(representation => representation.Description)
            .HasMaxLength(ArtistRepresentation.DescriptionMaxLength);

        // The agency is another party: no cascade, the conventions make it RESTRICT (database §10.2).
        builder.HasOne<Party>().WithMany().HasForeignKey(representation => representation.AgencyId);
    }
}
