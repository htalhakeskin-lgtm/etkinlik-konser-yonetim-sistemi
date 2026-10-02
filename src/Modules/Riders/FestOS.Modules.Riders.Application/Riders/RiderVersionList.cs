namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>
/// A rider's versions, newest first; <see cref="Version"/> is the rider's, also the <c>ETag</c>, which saving
/// the next version sends (BR-RDR-003).
/// </summary>
public sealed record RiderVersionList(IReadOnlyList<RiderVersionListItem> Versions, int Version);
