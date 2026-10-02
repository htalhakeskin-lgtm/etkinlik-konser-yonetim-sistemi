using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Riders.Domain.Productions;
using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;

namespace FestOS.Modules.Riders.UnitTests;

public sealed class RiderVersionTests
{
    private static readonly Guid Model = Guid.CreateVersion7();
    private static readonly Guid Other = Guid.CreateVersion7();
    private static readonly Guid Category = Guid.CreateVersion7();

    private static Rider NewRider() =>
        Rider.ForProduction(Production.Create(Guid.CreateVersion7(), "Akustik", null).Id);

    private static RiderLineDetails ModelLine(
        RiderLineFlexibility? flexibility = RiderLineFlexibility.Required,
        Guid? lineKey = null,
        params Guid[] equivalents
    ) => new(lineKey, Model, null, 2, flexibility, equivalents, null);

    [Fact]
    [Trait("Rule", "BR-RDR-003")]
    public void AddVersion_NumbersTheVersionsOneAfterAnother_AndRaisesTheEvent()
    {
        Rider rider = NewRider();

        RiderVersion first = rider.AddVersion([ModelLine()], " İlk ", "Teknik Müdür");
        Guid key = first.Lines[0].LineKey;
        RiderVersion second = rider.AddVersion(
            [ModelLine(lineKey: key), new(null, null, Category, 4, null, [], "Yedek")],
            null,
            "Teknik Müdür"
        );

        first.Number.ShouldBe(1);
        first.Note.ShouldBe("İlk");
        second.Number.ShouldBe(2);
        rider.LatestVersionNumber.ShouldBe(2);
        second.Lines[0].LineKey.ShouldBe(key);
        second.Lines[1].LineKey.ShouldNotBe(key);
        second.Lines[1].SortOrder.ShouldBe(1);
        rider.DomainEvents.OfType<RiderVersionCreatedDomainEvent>().Select(created => created.Number).ShouldBe([1, 2]);
    }

    [Theory]
    [Trait("Rule", "BR-RDR-001")]
    [InlineData("target")]
    [InlineData("flexibility")]
    [InlineData("categoryFlexibility")]
    public void AddVersion_RefusesALineWithoutOneTargetOrWithMisplacedFlexibility(string reason)
    {
        RiderLineDetails line = reason switch
        {
            "target" => new(null, Model, Category, 1, RiderLineFlexibility.Required, [], null),
            "flexibility" => ModelLine(flexibility: null),
            _ => new(null, null, Category, 1, RiderLineFlexibility.Flexible, [], null),
        };

        BusinessRuleViolationException refused = Should.Throw<BusinessRuleViolationException>(() =>
            NewRider().AddVersion([ModelLine(), line], null, "Teknik Müdür")
        );

        refused.RuleCode.ShouldBe("BR-RDR-001");
        refused.Parameters["line"].ShouldBe(1);
        refused
            .Parameters["reason"]
            .ShouldBe(string.Equals(reason, "target", StringComparison.Ordinal) ? "target" : "flexibility");
    }

    [Theory]
    [Trait("Rule", "BR-RDR-002")]
    [InlineData("notFlexible")]
    [InlineData("sameModel")]
    [InlineData("repeated")]
    public void AddVersion_RefusesEquivalentsOutsideAFlexibleLine_OrRepeatingAModel(string reason)
    {
        RiderLineDetails line = reason switch
        {
            "notFlexible" => ModelLine(RiderLineFlexibility.Required, null, Other),
            "sameModel" => ModelLine(RiderLineFlexibility.Flexible, null, Other, Model),
            _ => ModelLine(RiderLineFlexibility.Flexible, null, Other, Other),
        };

        BusinessRuleViolationException refused = Should.Throw<BusinessRuleViolationException>(() =>
            NewRider().AddVersion([line], null, "Teknik Müdür")
        );

        refused.RuleCode.ShouldBe("BR-RDR-002");
        refused.Parameters["reason"].ShouldBe(reason);
    }

    [Fact]
    [Trait("Rule", "BR-RDR-002")]
    public void AddVersion_KeepsTheEquivalentsOfAFlexibleLineInOrder()
    {
        var third = Guid.CreateVersion7();

        RiderVersion version = NewRider()
            .AddVersion([ModelLine(RiderLineFlexibility.Flexible, null, third, Other)], null, "TM");

        version.Lines[0].Equivalents.Select(equivalent => equivalent.ModelId).ShouldBe([third, Other]);
        version.Lines[0].Equivalents.Select(equivalent => equivalent.SortOrder).ShouldBe([0, 1]);
    }
}
