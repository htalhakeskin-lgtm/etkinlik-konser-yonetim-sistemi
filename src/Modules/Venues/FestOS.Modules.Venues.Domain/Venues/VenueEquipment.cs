using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Rules;

namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>
/// Equipment the venue has and lends to events (US-VEN-002): a model, a category or a free description,
/// with a quantity, optional validity days and unavailability periods; part of the venue (06 §7).
/// </summary>
public sealed class VenueEquipment : Entity<VenueEquipmentId>
{
    /// <summary>The longest free description.</summary>
    public const int DescriptionMaxLength = 200;

    private readonly List<VenueEquipmentUnavailability> _unavailabilities = [];

    internal VenueEquipment(VenueEquipmentId id, VenueEquipmentDetails details)
        : this(id) => Change(details);

    private VenueEquipment(VenueEquipmentId id)
        : base(id) { }

    /// <summary>The catalog model, another module's record by identifier.</summary>
    public Guid? ModelId { get; private set; }

    /// <summary>The catalog category, another module's record by identifier.</summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>Equipment the catalog lacks, in words.</summary>
    public string? Description { get; private set; }

    /// <summary>How many.</summary>
    public int Quantity { get; private set; }

    /// <summary>The first day it is there, or open towards the past.</summary>
    public DateOnly? ValidityStart { get; private set; }

    /// <summary>The day it stops being there, not included, or open towards the future.</summary>
    public DateOnly? ValidityEnd { get; private set; }

    /// <summary>The days when part of it cannot be used, in no particular order.</summary>
    public IReadOnlyList<VenueEquipmentUnavailability> Unavailabilities => _unavailabilities;

    /// <summary>Whether a requirement calculation counts it: only model and category lines do (BR-VEN-001).</summary>
    public bool IsCounted => ModelId is not null || CategoryId is not null;

    /// <summary>
    /// How many an event from <paramref name="start"/> up to <paramref name="end"/> can use (BR-VEN-001): none
    /// unless the line is there the whole time, else its quantity less the largest unavailable quantity of
    /// the periods that overlap the event. Periods never overlap each other (BR-VEN-002), so the largest one
    /// is the most missing at once.
    /// </summary>
    public int UsableQuantity(DateTimeOffset start, DateTimeOffset end, TimeZoneInfo zone)
    {
        if (!IsCounted)
        {
            return 0;
        }

        bool isThere =
            (ValidityStart is not { } from || VenueDays.StartOf(from, zone) <= start)
            && (ValidityEnd is not { } until || end <= VenueDays.StartOf(until, zone));
        if (!isThere)
        {
            return 0;
        }

        int missing = _unavailabilities
            .Where(period =>
                VenueDays.StartOf(period.PeriodStart, zone) < end && start < VenueDays.StartOf(period.PeriodEnd, zone)
            )
            .Select(period => period.Quantity)
            .DefaultIfEmpty(0)
            .Max();
        return Math.Max(0, Quantity - missing);
    }

    internal void Change(VenueEquipmentDetails details)
    {
        int targets =
            (details.ModelId is null ? 0 : 1)
            + (details.CategoryId is null ? 0 : 1)
            + (string.IsNullOrWhiteSpace(details.Description) ? 0 : 1);
        if (targets != 1)
        {
            throw new BusinessRuleViolationException(
                VenuesRuleCodes.EquipmentTarget,
                "A line asks for exactly one of a model, a category or a free description."
            );
        }

        if (details.ValidityStart is { } from && details.ValidityEnd is { } until && until <= from)
        {
            throw Periods("A line's validity ends after it starts.", "validity");
        }

        foreach (VenueEquipmentUnavailability period in _unavailabilities)
        {
            EnsureFits(
                period.PeriodStart,
                period.PeriodEnd,
                period.Quantity,
                details.Quantity,
                details.ValidityStart,
                details.ValidityEnd
            );
        }

        ModelId = details.ModelId;
        CategoryId = details.CategoryId;
        Description = string.IsNullOrWhiteSpace(details.Description) ? null : details.Description.Trim();
        Quantity = details.Quantity;
        ValidityStart = details.ValidityStart;
        ValidityEnd = details.ValidityEnd;
    }

    internal VenueEquipmentUnavailability AddUnavailability(UnavailabilityDetails details)
    {
        EnsurePeriodFits(details, except: null);
        var period = new VenueEquipmentUnavailability(VenueEquipmentUnavailabilityId.New(), details);
        _unavailabilities.Add(period);
        return period;
    }

    internal VenueEquipmentUnavailability ChangeUnavailability(
        VenueEquipmentUnavailabilityId id,
        UnavailabilityDetails details
    )
    {
        VenueEquipmentUnavailability period = Period(id);
        EnsurePeriodFits(details, except: id);
        period.Change(details);
        return period;
    }

    internal VenueEquipmentUnavailability RemoveUnavailability(VenueEquipmentUnavailabilityId id)
    {
        VenueEquipmentUnavailability period = Period(id);
        _unavailabilities.Remove(period);
        return period;
    }

    internal VenueEquipmentUnavailability Period(VenueEquipmentUnavailabilityId id) =>
        _unavailabilities.Find(period => period.Id == id) ?? throw new KeyNotFoundException("No such period.");

    // A period lies inside the line's validity, misses no more than the line has, and overlaps no other
    // period of the line (BR-VEN-002).
    private void EnsurePeriodFits(UnavailabilityDetails details, VenueEquipmentUnavailabilityId? except)
    {
        if (details.PeriodEnd <= details.PeriodStart)
        {
            throw Periods("A period ends after it starts.", "period");
        }

        EnsureFits(details.PeriodStart, details.PeriodEnd, details.Quantity, Quantity, ValidityStart, ValidityEnd);
        if (
            _unavailabilities.Any(period =>
                period.Id != except && period.Overlaps(details.PeriodStart, details.PeriodEnd)
            )
        )
        {
            throw Periods("A line's periods do not overlap.", "overlap");
        }
    }

    private static void EnsureFits(
        DateOnly periodStart,
        DateOnly periodEnd,
        int missing,
        int quantity,
        DateOnly? validityStart,
        DateOnly? validityEnd
    )
    {
        if (missing > quantity)
        {
            throw Periods("A period misses no more than the line has.", "quantity");
        }

        if ((validityStart is { } from && periodStart < from) || (validityEnd is { } until && until < periodEnd))
        {
            throw Periods("A period lies inside the line's validity.", "outsideValidity");
        }
    }

    private static BusinessRuleViolationException Periods(string message, string reason) =>
        new(
            VenuesRuleCodes.EquipmentPeriods,
            message,
            parameters: new Dictionary<string, object?>(StringComparer.Ordinal) { ["reason"] = reason }
        );
}
