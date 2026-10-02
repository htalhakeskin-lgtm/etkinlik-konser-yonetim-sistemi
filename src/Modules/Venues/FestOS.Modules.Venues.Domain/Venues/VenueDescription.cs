namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>A venue's details as the form sends them (US-VEN-001).</summary>
/// <param name="Name">The name, unique in its city.</param>
/// <param name="City">The city.</param>
/// <param name="Address">The address.</param>
/// <param name="OperatorPartyId">The party running it, if known (BR-PTY-004).</param>
/// <param name="Capacity">The largest audience allowed.</param>
/// <param name="StageWidthMeters">Stage width in meters.</param>
/// <param name="StageDepthMeters">Stage depth in meters.</param>
/// <param name="StageHeightMeters">Stage height in meters.</param>
/// <param name="LoadingDock">How equipment gets in.</param>
/// <param name="PowerCapacityAmperes">The power it supplies, in amperes; recorded in S1, used in S2.</param>
/// <param name="Curfew">The latest local time sound may run.</param>
/// <param name="TimeZone">The IANA time zone of its local times (database §7.2).</param>
public sealed record VenueDescription(
    string Name,
    string City,
    string Address,
    Guid? OperatorPartyId,
    int Capacity,
    decimal? StageWidthMeters,
    decimal? StageDepthMeters,
    decimal? StageHeightMeters,
    string? LoadingDock,
    decimal? PowerCapacityAmperes,
    TimeOnly? Curfew,
    string TimeZone
);
