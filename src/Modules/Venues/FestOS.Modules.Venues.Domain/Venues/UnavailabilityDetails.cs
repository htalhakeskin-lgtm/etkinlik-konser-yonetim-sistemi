namespace FestOS.Modules.Venues.Domain.Venues;

/// <summary>A period when part of a line cannot be used, as the form sends it (US-VEN-002).</summary>
/// <param name="PeriodStart">The first day.</param>
/// <param name="PeriodEnd">The day after the last one, not included (database §7.3).</param>
/// <param name="Quantity">How many cannot be used.</param>
/// <param name="Reason">Why, e.g. lent to another event.</param>
public sealed record UnavailabilityDetails(DateOnly PeriodStart, DateOnly PeriodEnd, int Quantity, string Reason);
