using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Parties.Infrastructure;

internal sealed class ContactPointConfiguration : IEntityTypeConfiguration<ContactPoint>
{
    public void Configure(EntityTypeBuilder<ContactPoint> builder)
    {
        builder.ToTable("contact_points");
        builder.Property(contactPoint => contactPoint.Value).HasMaxLength(ContactPoint.ValueMaxLength);
        builder.Property(contactPoint => contactPoint.Label).HasMaxLength(ContactPoint.LabelMaxLength);

        // One primary per kind is kept by the party (BR-PTY-002). A partial unique index cannot be deferred,
        // and moving the primary updates two rows in one save, so it would fail in between (parties §5).
    }
}
