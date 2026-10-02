namespace FestOS.Modules.Catalog.Application.Kits;

/// <summary>
/// A kit's weight and power, its contents opened down to models (BR-EQP-003). A total is incomplete when a
/// model in it has no value; it still sums the known ones.
/// </summary>
/// <param name="WeightKilograms">The total weight.</param>
/// <param name="IsWeightComplete">Whether every model has a weight.</param>
/// <param name="PowerWatts">The total power draw.</param>
/// <param name="IsPowerComplete">Whether every model has a power value.</param>
public sealed record KitTotals(decimal WeightKilograms, bool IsWeightComplete, int PowerWatts, bool IsPowerComplete);
