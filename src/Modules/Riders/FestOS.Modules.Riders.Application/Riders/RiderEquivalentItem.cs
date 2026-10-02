namespace FestOS.Modules.Riders.Application.Riders;

/// <summary>An equivalent model with its name, in order.</summary>
public sealed record RiderEquivalentItem(Guid ModelId, string Name, bool IsActive);
