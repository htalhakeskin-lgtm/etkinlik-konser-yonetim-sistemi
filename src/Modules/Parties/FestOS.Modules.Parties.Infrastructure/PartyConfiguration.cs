using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Parties.Infrastructure;

internal sealed class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> builder)
    {
        builder.ToTable("parties");
        builder.Property(party => party.Name).HasMaxLength(Party.NameMaxLength);
        builder.Property(party => party.FirstName).HasMaxLength(Party.PersonNameMaxLength);
        builder.Property(party => party.LastName).HasMaxLength(Party.PersonNameMaxLength);
        builder.Property(party => party.LegalName).HasMaxLength(Party.NameMaxLength);

        // A person has a first and last name and no legal name; an organization only a legal name, if any
        // (parties PT-01).
        builder.ToTable(table =>
            table.HasCheckConstraint(
                "ck_parties_kind_names",
                "(kind = 'person' AND first_name IS NOT NULL AND last_name IS NOT NULL AND legal_name IS NULL)"
                    + " OR (kind = 'organization' AND first_name IS NULL AND last_name IS NULL)"
            )
        );

        // Roles are an array on the party, like a user's roles (parties PT-03); GIN answers "who is an artist".
        builder
            .PrimitiveCollection(party => party.Roles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .ElementType(role => role.HasConversion<CamelCaseEnumConverter<PartyRole>>());
        builder.HasIndex(party => party.Roles).HasMethod("gin").HasDatabaseName("ix_parties_roles");
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "ck_parties_roles_enum",
                $"roles <@ ARRAY[{string.Join(", ", CamelCaseEnumConverter<PartyRole>.StoredValues.Select(value => $"'{value}'"))}]::text[]"
            );
            table.HasCheckConstraint("ck_parties_roles_present", "cardinality(roles) > 0");
        });

        // Searching inside names and contact points needs trigrams (database §13, parties PT-04).
        builder.Property(party => party.Search).HasColumnType("text");
        builder
            .HasIndex(party => party.Search)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("ix_parties_search");

        // Contact points belong to the party, so this cascade is explicit (database §10.2).
        builder
            .HasMany(party => party.ContactPoints)
            .WithOne()
            .HasForeignKey("PartyId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(party => party.ContactPoints).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
