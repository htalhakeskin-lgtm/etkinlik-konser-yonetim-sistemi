using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Parties.Domain;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.UnitTests;

public sealed class PartyTests
{
    private static readonly DateTimeOffset At = new(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Rule", "BR-PTY-001")]
    public void CreatePerson_KeepsTheNamesAndEachRoleOnce()
    {
        var party = Party.CreatePerson(
            " Tarkan ",
            " Hüseyin Tarkan ",
            " Tevetoğlu ",
            [PartyRole.Artist, PartyRole.Customer, PartyRole.Artist],
            []
        );

        party.Kind.ShouldBe(PartyKind.Person);
        party.Name.ShouldBe("Tarkan");
        party.FirstName.ShouldBe("Hüseyin Tarkan");
        party.LastName.ShouldBe("Tevetoğlu");
        party.LegalName.ShouldBeNull();
        party.Roles.ShouldBe([PartyRole.Customer, PartyRole.Artist]);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-001")]
    public void Create_WithoutARole_IsRefused()
    {
        BusinessRuleViolationException refusal = Should.Throw<BusinessRuleViolationException>(() =>
            Party.CreateOrganization("Işık Ses", null, [], [])
        );

        refusal.RuleCode.ShouldBe(PartiesRuleCodes.PartyRoles);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-001")]
    public void Edit_KeepsTheKindAndDropsNamesThatDoNotBelongToIt()
    {
        var party = Party.CreateOrganization("Işık Ses", "Işık Ses Sistemleri A.Ş.", [PartyRole.Supplier], []);

        party.Edit("Işık Ses", "Ayşe", "Kaya", null, [PartyRole.Supplier, PartyRole.Customer], []);

        party.Kind.ShouldBe(PartyKind.Organization);
        party.FirstName.ShouldBeNull();
        party.LastName.ShouldBeNull();
        party.LegalName.ShouldBeNull();
        party.Roles.ShouldBe([PartyRole.Customer, PartyRole.Supplier]);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-002")]
    public void ContactPoints_TheFirstOfEachKindIsPrimaryWhenNoneIsMarked()
    {
        var party = Party.CreateOrganization(
            "Işık Ses",
            null,
            [PartyRole.Supplier],
            [Phone("0212 111 22 33"), Phone("0532 444 55 66"), Email("INFO@isikses.example")]
        );

        party
            .ContactPoints.Select(point => (point.Value, point.IsPrimary))
            .ShouldBe([("0212 111 22 33", true), ("0532 444 55 66", false), ("info@isikses.example", true)]);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-002")]
    public void ContactPoints_TwoPrimariesOfOneKind_AreRefused()
    {
        BusinessRuleViolationException refusal = Should.Throw<BusinessRuleViolationException>(() =>
            Party.CreateOrganization(
                "Işık Ses",
                null,
                [PartyRole.Supplier],
                [Phone("0212 111 22 33", isPrimary: true), Phone("0532 444 55 66", isPrimary: true)]
            )
        );

        refusal.RuleCode.ShouldBe(PartiesRuleCodes.PrimaryContactPoint);
    }

    [Fact]
    [Trait("Rule", "BR-PTY-002")]
    public void Edit_UpdatesKeptContactPoints_AddsNewOnes_AndMovesThePrimaryWhenItIsRemoved()
    {
        var party = Party.CreateOrganization(
            "Işık Ses",
            null,
            [PartyRole.Supplier],
            [Phone("0212 111 22 33"), Phone("0532 444 55 66")]
        );
        ContactPoint first = party.ContactPoints[0];
        ContactPoint second = party.ContactPoints[1];

        party.Edit(
            "Işık Ses",
            null,
            null,
            null,
            [PartyRole.Supplier],
            [Phone("0532 444 55 67", id: second.Id), Email("satis@isikses.example")]
        );

        party.ContactPoints.ShouldNotContain(first);
        party.ContactPoints[0].ShouldBeSameAs(second);
        second.Value.ShouldBe("0532 444 55 67");
        second.IsPrimary.ShouldBeTrue("the primary was removed, so the next phone takes its place");
        party.ContactPoints[1].Kind.ShouldBe(ContactPointKind.Email);
    }

    [Fact]
    public void Search_FindsTheNamesAndTheContactPoints_PhonesAlsoAsDigits()
    {
        var party = Party.CreatePerson(
            "Şebnem Ilgaz",
            "Şebnem",
            "Ilgaz",
            [PartyRole.Contact],
            [Phone("+90 (532) 111-22-33"), Email("sebnem@example.com")]
        );

        party.Search.ShouldContain("sebnem ilgaz");
        party.Search.ShouldContain("905321112233");
        party.Search.ShouldContain("sebnem@example.com");
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public void Deactivate_KeepsTheFirstMoment_AndActivateOpensItAgain()
    {
        var party = Party.CreateOrganization("Işık Ses", null, [PartyRole.Supplier], []);
        var by = Guid.CreateVersion7();

        party.Deactivate(by, At);
        party.Deactivate(Guid.CreateVersion7(), At.AddDays(1));

        party.DeactivatedAt.ShouldBe(At);
        party.DeactivatedBy.ShouldBe(by);
        party.Activate();
        party.DeactivatedAt.ShouldBeNull();
    }

    private static ContactPointDetails Phone(string value, bool isPrimary = false, ContactPointId? id = null) =>
        new(id, ContactPointKind.Phone, value, null, isPrimary);

    private static ContactPointDetails Email(string value) => new(null, ContactPointKind.Email, value, null, false);
}
