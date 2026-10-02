using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.UnitTests;

public sealed class VenueTests
{
    private static VenueDescription Description(string name, string city) =>
        new(
            name,
            city,
            " Harbiye Mah. 1 ",
            null,
            4000,
            18m,
            12m,
            null,
            "  ",
            null,
            new TimeOnly(23, 0),
            "Europe/Istanbul"
        );

    [Fact]
    [Trait("Rule", "BR-VEN-003")]
    public void Create_KeepsTheNameAndCityWithTheirComparedForms()
    {
        var venue = Venue.Create(Description(" Açıkhava Tiyatrosu ", " İstanbul "));

        venue.Name.ShouldBe("Açıkhava Tiyatrosu");
        venue.NameSearch.ShouldBe("acikhava tiyatrosu");
        venue.City.ShouldBe("İstanbul");
        venue.CitySearch.ShouldBe("istanbul");
        venue.Address.ShouldBe("Harbiye Mah. 1");
        venue.LoadingDock.ShouldBeNull();
        venue.Curfew.ShouldBe(new TimeOnly(23, 0));
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public void Deactivate_KeepsTheFirstMoment_AndActivateOpensItAgain()
    {
        var venue = Venue.Create(Description("Açıkhava", "İstanbul"));
        var at = new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);
        var by = Guid.CreateVersion7();

        venue.Deactivate(by, at);
        venue.Deactivate(Guid.CreateVersion7(), at.AddDays(1));

        venue.DeactivatedAt.ShouldBe(at);
        venue.DeactivatedBy.ShouldBe(by);
        venue.Activate();
        venue.DeactivatedAt.ShouldBeNull();
    }
}
