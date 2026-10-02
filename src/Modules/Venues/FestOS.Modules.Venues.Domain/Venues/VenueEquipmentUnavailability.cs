using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>Days when part of a <see cref="VenueEquipment"/> line cannot be used (06 decision 16).</summary>
public sealed class VenueEquipmentUnavailability : Entity<VenueEquipmentUnavailabilityId>
{
    /// <summary>The longest reason.</summary>
    public const int ReasonMaxLength = 500;

    internal VenueEquipmentUnavailability(VenueEquipmentUnavailabilityId id, UnavailabilityDetails details)
        : this(id) => Change(details);

    private VenueEquipmentUnavailability(VenueEquipmentUnavailabilityId id)
        : base(id) { }

    /// <summary>The first day.</summary>
    public DateOnly PeriodStart { get; private set; }

    /// <summary>The day after the last one, not included.</summary>
    public DateOnly PeriodEnd { get; private set; }

    /// <summary>How many cannot be used.</summary>
    public int Quantity { get; private set; }

    /// <summary>Why.</summary>
    public string Reason { get; private set; } = string.Empty;

    internal bool Overlaps(DateOnly start, DateOnly end) => PeriodStart < end && start < PeriodEnd;

    internal void Change(UnavailabilityDetails details)
    {
        PeriodStart = details.PeriodStart;
        PeriodEnd = details.PeriodEnd;
        Quantity = details.Quantity;
        Reason = details.Reason.Trim();
    }
}
