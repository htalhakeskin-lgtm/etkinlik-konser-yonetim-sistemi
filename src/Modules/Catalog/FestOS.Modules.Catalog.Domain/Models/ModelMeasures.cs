namespace FestOS.Modules.Catalog.Domain.Models;

/// <summary>A model's optional technical values (US-EQP-002); the power and loading calculations use them later.</summary>
/// <param name="WeightKilograms">Weight in kilograms.</param>
/// <param name="PowerWatts">Power draw in watts.</param>
/// <param name="TransportVolumeCubicMeters">Volume when packed for transport, in cubic meters.</param>
public sealed record ModelMeasures(decimal? WeightKilograms, int? PowerWatts, decimal? TransportVolumeCubicMeters);
