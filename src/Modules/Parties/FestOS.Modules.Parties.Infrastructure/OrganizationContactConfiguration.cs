using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Parties.Infrastructure;

internal sealed class OrganizationContactConfiguration : IEntityTypeConfiguration<OrganizationContact>
{
    public void Configure(EntityTypeBuilder<OrganizationContact> builder)
    {
        builder.ToTable("organization_contacts");
        builder.Property(contact => contact.Title).HasMaxLength(OrganizationContact.TitleMaxLength);

        // The person is another party: no cascade, the conventions make it RESTRICT (database §10.2).
        builder.HasOne<Party>().WithMany().HasForeignKey(contact => contact.PersonId);
    }
}
