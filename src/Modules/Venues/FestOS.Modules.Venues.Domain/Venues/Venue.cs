using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Text;

namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>
/// A place where events happen, with its technical details (US-VEN-001). Holds and events refer to it, so
/// it is never deleted, only deactivated (BR-SYS-001). Its own equipment joins in the next step.
/// </summary>
public sealed class Venue : AggregateRoot<VenueId>, IDeactivatable
{
    /// <summary>The longest name.</summary>
    public const int NameMaxLength = 200;

    /// <summary>The longest city name.</summary>
    public const int CityMaxLength = 100;

    /// <summary>The longest address.</summary>
    public const int AddressMaxLength = 500;

    /// <summary>The longest loading dock description.</summary>
    public const int LoadingDockMaxLength = 2000;

    /// <summary>The longest time zone identifier.</summary>
    public const int TimeZoneMaxLength = 64;

    /// <summary>The time zone of a venue unless told otherwise (database §7.2).</summary>
    public const string DefaultTimeZone = "Europe/Istanbul";

    private Venue(VenueId id)
        : base(id) { }

    /// <summary>The name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The name's search key; with the city's, what makes two venues the same (BR-VEN-003).</summary>
    [NotAudited]
    public string NameSearch { get; private set; } = string.Empty;

    /// <summary>The city.</summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>The city's search key.</summary>
    [NotAudited]
    public string CitySearch { get; private set; } = string.Empty;

    /// <summary>The address.</summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>The party running it; another module's record, by identifier (05 §2, principle 7).</summary>
    public Guid? OperatorPartyId { get; private set; }

    /// <summary>The largest audience allowed.</summary>
    public int Capacity { get; private set; }

    /// <summary>Stage width in meters.</summary>
    public decimal? StageWidthMeters { get; private set; }

    /// <summary>Stage depth in meters.</summary>
    public decimal? StageDepthMeters { get; private set; }

    /// <summary>Stage height in meters.</summary>
    public decimal? StageHeightMeters { get; private set; }

    /// <summary>How equipment gets in.</summary>
    public string? LoadingDock { get; private set; }

    /// <summary>The power it supplies, in amperes.</summary>
    public decimal? PowerCapacityAmperes { get; private set; }

    /// <summary>The latest local time sound may run.</summary>
    public TimeOnly? Curfew { get; private set; }

    /// <summary>The IANA time zone of its local times.</summary>
    public string TimeZone { get; private set; } = DefaultTimeZone;

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>A new, active venue; the command checks the operator (BR-PTY-004).</summary>
    public static Venue Create(VenueDescription description)
    {
        var venue = new Venue(VenueId.New());
        venue.Edit(description);
        return venue;
    }

    /// <summary>Changes the venue's details.</summary>
    public void Edit(VenueDescription description)
    {
        Name = description.Name.Trim();
        NameSearch = SearchKey.Of(description.Name);
        City = description.City.Trim();
        CitySearch = SearchKey.Of(description.City);
        Address = description.Address.Trim();
        OperatorPartyId = description.OperatorPartyId;
        Capacity = description.Capacity;
        StageWidthMeters = description.StageWidthMeters;
        StageDepthMeters = description.StageDepthMeters;
        StageHeightMeters = description.StageHeightMeters;
        LoadingDock = string.IsNullOrWhiteSpace(description.LoadingDock) ? null : description.LoadingDock.Trim();
        PowerCapacityAmperes = description.PowerCapacityAmperes;
        Curfew = description.Curfew;
        TimeZone = description.TimeZone.Trim();
    }

    /// <summary>Takes the venue out of new selections (BR-SYS-001).</summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
        }
    }

    /// <summary>Opens a deactivated venue again.</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }
}
