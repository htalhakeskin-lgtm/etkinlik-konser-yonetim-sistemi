namespace FestOS.Modules.Riders.Infrastructure.Riders;

/// <summary>The body of <c>POST /api/v1/riders/{riderId}/versions</c>: all the lines and an optional note.</summary>
public sealed record CreateRiderVersionRequest(IReadOnlyList<RiderLineRequest> Lines, string? Note = null);
