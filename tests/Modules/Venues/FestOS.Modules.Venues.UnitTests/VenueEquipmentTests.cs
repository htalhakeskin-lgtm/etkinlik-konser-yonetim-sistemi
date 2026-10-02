using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.Modules.Venues.Domain;
using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.UnitTests;

public sealed class VenueEquipmentTests
{
    private static readonly TimeZoneInfo Istanbul = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    private static readonly Guid Console = Guid.CreateVersion7();

    private static Venue AVenue() =>
        Venue.Create(
            new VenueDescription(
                "Açıkhava",
                "İstanbul",
                "Harbiye",
                null,
                4000,
                null,
                null,
                null,
                null,
                null,
                null,
                "Europe/Istanbul"
            )
        );

    private static DateOnly Day(int month, int day) => new(2027, month, day);

    // An event from the evening of the first day to the early hours of the day after the last.
    private static (DateTimeOffset Start, DateTimeOffset End) Event(DateOnly first, DateOnly last) =>
        (VenueDays.StartOf(first, Istanbul).AddHours(18), VenueDays.StartOf(last.AddDays(1), Istanbul).AddHours(2));

    [Fact]
    [Trait("Rule", "BR-VEN-001")]
    public void UsableQuantity_IsTheQuantityLessTheLargestOverlappingPeriod()
    {
        Venue venue = AVenue();
        VenueEquipment line = venue.AddEquipment(new(Console, null, null, 4, null, null));
        venue.AddUnavailability(line.Id, new(Day(3, 10), Day(3, 12), 1, "Başka etkinlik"));
        venue.AddUnavailability(line.Id, new(Day(3, 12), Day(3, 14), 3, "Bakım"));
        (DateTimeOffset start, DateTimeOffset end) = Event(Day(3, 11), Day(3, 12));

        line.UsableQuantity(start, end, Istanbul).ShouldBe(1, "3 missing at most at once, not 1 + 3");
        (DateTimeOffset laterStart, DateTimeOffset laterEnd) = Event(Day(3, 20), Day(3, 20));
        line.UsableQuantity(laterStart, laterEnd, Istanbul).ShouldBe(4);
    }

    [Fact]
    [Trait("Rule", "BR-VEN-001")]
    public void UsableQuantity_IsNoneUnlessTheLineIsThereTheWholeEvent_AndFreeDescriptionsNeverCount()
    {
        Venue venue = AVenue();
        VenueEquipment newConsole = venue.AddEquipment(new(Console, null, null, 1, Day(3, 12), null));
        VenueEquipment piano = venue.AddEquipment(new(null, null, "Kuyruklu piyano", 1, null, null));
        (DateTimeOffset start, DateTimeOffset end) = Event(Day(3, 11), Day(3, 12));

        newConsole.UsableQuantity(start, end, Istanbul).ShouldBe(0, "it arrives during the event");
        piano.IsCounted.ShouldBeFalse();
        piano.UsableQuantity(start, end, Istanbul).ShouldBe(0);
    }

    [Fact]
    [Trait("Rule", "BR-VEN-002")]
    public void Periods_StayInsideTheValidity_MissNoMoreThanTheLine_AndDoNotOverlap()
    {
        Venue venue = AVenue();
        VenueEquipment line = venue.AddEquipment(new(Console, null, null, 2, Day(3, 1), Day(4, 1)));
        venue.AddUnavailability(line.Id, new(Day(3, 10), Day(3, 12), 2, "Ödünç"));

        Reason(() => venue.AddUnavailability(line.Id, new(Day(2, 25), Day(3, 2), 1, "Erken")))
            .ShouldBe("outsideValidity");
        Reason(() => venue.AddUnavailability(line.Id, new(Day(3, 20), Day(3, 21), 3, "Fazla"))).ShouldBe("quantity");
        Reason(() => venue.AddUnavailability(line.Id, new(Day(3, 11), Day(3, 13), 1, "Çakışan"))).ShouldBe("overlap");
        Reason(() => venue.EditEquipment(line.Id, new(Console, null, null, 1, Day(3, 1), Day(4, 1))))
            .ShouldBe("quantity");
        Reason(() => venue.EditEquipment(line.Id, new(Console, null, null, 2, Day(3, 11), Day(4, 1))))
            .ShouldBe("outsideValidity");
    }

    [Fact]
    [Trait("Rule", "BR-VEN-001")]
    public void ALine_AsksForExactlyOneTarget()
    {
        Should
            .Throw<BusinessRuleViolationException>(() =>
                AVenue().AddEquipment(new(Console, Guid.CreateVersion7(), null, 1, null, null))
            )
            .RuleCode.ShouldBe(VenuesRuleCodes.EquipmentTarget);
    }

    [Fact]
    public void Changes_TellTheDaysTheyTouch()
    {
        Venue venue = AVenue();
        VenueEquipment line = venue.AddEquipment(new(Console, null, null, 2, Day(3, 1), Day(4, 1)));
        venue.DequeueDomainEvents();

        venue.EditEquipment(line.Id, new(Console, null, null, 2, Day(3, 5), Day(5, 1)));

        venue
            .DequeueDomainEvents()
            .ShouldHaveSingleItem()
            .ShouldBe(new VenueEquipmentChangedDomainEvent(venue.Id, Day(3, 1), Day(5, 1)));
    }

    private static string? Reason(Action change) =>
        Should.Throw<BusinessRuleViolationException>(change).Parameters["reason"] as string;
}
