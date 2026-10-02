using FestOS.Modules.Venues.Domain.Venues;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

/// <summary>The body of an unavailability period: its days (the end not included), quantity and reason.</summary>
public sealed record UnavailabilityRequest(DateOnly PeriodStart, DateOnly PeriodEnd, int Quantity, string Reason)
{
    /// <summary>The period as the line takes it.</summary>
    public UnavailabilityDetails Details() => new(PeriodStart, PeriodEnd, Quantity, Reason);
}
