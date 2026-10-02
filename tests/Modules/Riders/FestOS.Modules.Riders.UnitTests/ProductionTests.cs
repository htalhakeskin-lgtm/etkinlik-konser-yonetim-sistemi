using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;

namespace FestOS.Modules.Riders.UnitTests;

public sealed class ProductionTests
{
    [Fact]
    [Trait("Rule", "BR-RDR-009")]
    public void Create_KeepsTheNameWithItsComparedForm()
    {
        var artist = Guid.CreateVersion7();

        var production = Production.Create(artist, " Gece Turnesi 2027 ", "   ");

        production.ArtistPartyId.ShouldBe(artist);
        production.Name.ShouldBe("Gece Turnesi 2027");
        production.NameSearch.ShouldBe("gece turnesi 2027");
        production.Description.ShouldBeNull();
    }

    [Fact]
    [Trait("Rule", "BR-SYS-001")]
    public void Deactivate_KeepsTheFirstMoment_AndActivateOpensItAgain()
    {
        var production = Production.Create(Guid.CreateVersion7(), "Akustik", null);
        var at = new DateTimeOffset(2027, 1, 4, 6, 0, 0, TimeSpan.Zero);
        var by = Guid.CreateVersion7();

        production.Deactivate(by, at);
        production.Deactivate(Guid.CreateVersion7(), at.AddDays(1));

        production.DeactivatedAt.ShouldBe(at);
        production.DeactivatedBy.ShouldBe(by);
        production.Activate();
        production.DeactivatedAt.ShouldBeNull();
    }

    [Fact]
    [Trait("Rule", "BR-RDR-007")]
    public void ForProduction_OpensAnEmptyRiderThatComesFromTheProduction()
    {
        var production = Production.Create(Guid.CreateVersion7(), "Akustik", null);

        var rider = Rider.ForProduction(production.Id);

        rider.Source.ShouldBe(RiderSource.Production);
        rider.ProductionId.ShouldBe(production.Id);
        rider.LatestVersionNumber.ShouldBe(0);
    }
}
