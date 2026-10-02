using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Parties.Domain;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.UnitTests;

public sealed class PartyLinkTests
{
    private static readonly DateTimeOffset At = new(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Rule", "BR-PTY-003")]
    public void AddContactPerson_TiesAnActivePersonToAnOrganizationOnce()
    {
        Party organization = Organization(PartyRole.VenueOperator);
        Party person = Person(PartyRole.Contact);

        OrganizationContact contact = organization.AddContactPerson(person, " Teknik sorumlu ");

        contact.PersonId.ShouldBe(person.Id);
        contact.Title.ShouldBe("Teknik sorumlu");
        Should
            .Throw<BusinessRuleViolationException>(() => organization.AddContactPerson(person, null))
            .RuleCode.ShouldBe(PartiesRuleCodes.ContactPerson);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-003")]
    public void AddContactPerson_RefusesAPersonForAPerson_AnOrganizationAsTheContact_AndAnInactivePerson()
    {
        Party inactive = Person(PartyRole.Contact);
        inactive.Deactivate(Guid.CreateVersion7(), At);

        Should
            .Throw<BusinessRuleViolationException>(() =>
                Person(PartyRole.Customer).AddContactPerson(Person(PartyRole.Contact), null)
            )
            .RuleCode.ShouldBe(PartiesRuleCodes.ContactPerson);
        Should
            .Throw<BusinessRuleViolationException>(() =>
                Organization(PartyRole.Supplier).AddContactPerson(Organization(PartyRole.Supplier), null)
            )
            .RuleCode.ShouldBe(PartiesRuleCodes.ContactPerson);
        Should
            .Throw<BusinessRuleViolationException>(() =>
                Organization(PartyRole.Supplier).AddContactPerson(inactive, null)
            )
            .RuleCode.ShouldBe(PartiesRuleCodes.ContactPerson);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-004")]
    public void AddRepresentation_NeedsAnArtistAndAnActiveAgency_EachOnce()
    {
        Party artist = Person(PartyRole.Artist);
        Party agency = Organization(PartyRole.Agency);
        Party supplier = Organization(PartyRole.Supplier);

        ArtistRepresentation representation = artist.AddRepresentation(agency, "Avrupa");

        representation.AgencyId.ShouldBe(agency.Id);
        representation.Description.ShouldBe("Avrupa");
        Should
            .Throw<BusinessRuleViolationException>(() => artist.AddRepresentation(supplier, null))
            .RuleCode.ShouldBe(PartiesRuleCodes.RoleRequiredSelection);
        Should
            .Throw<BusinessRuleViolationException>(() => artist.AddRepresentation(agency, null))
            .RuleCode.ShouldBe(PartiesRuleCodes.RoleRequiredSelection);
        Should
            .Throw<BusinessRuleViolationException>(() => Person(PartyRole.Customer).AddRepresentation(agency, null))
            .RuleCode.ShouldBe(PartiesRuleCodes.RoleRequiredSelection);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-004")]
    public void Edit_KeepsTheArtistRoleWhileThereAreRepresentations()
    {
        Party artist = Person(PartyRole.Artist);
        ArtistRepresentation representation = artist.AddRepresentation(Organization(PartyRole.Agency), null);

        Should
            .Throw<BusinessRuleViolationException>(() =>
                artist.Edit(artist.Name, "Ayşe", "Kaya", null, [PartyRole.Customer], [])
            )
            .RuleCode.ShouldBe(PartiesRuleCodes.RoleRequiredSelection);

        artist.RemoveRepresentation(representation.Id);
        artist.Edit(artist.Name, "Ayşe", "Kaya", null, [PartyRole.Customer], []);
        artist.Roles.ShouldBe([PartyRole.Customer]);
    }

    private static Party Person(PartyRole role) => Party.CreatePerson("Ayşe Kaya", "Ayşe", "Kaya", [role], []);

    private static Party Organization(PartyRole role) => Party.CreateOrganization("Işık Ses", null, [role], []);
}
