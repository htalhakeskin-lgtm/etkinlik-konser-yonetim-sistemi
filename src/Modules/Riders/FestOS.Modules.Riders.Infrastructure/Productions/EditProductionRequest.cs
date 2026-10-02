namespace FestOS.Modules.Riders.Infrastructure.Productions;

/// <summary>The body of <c>PUT /api/v1/productions/{productionId}</c>; the artist does not change.</summary>
public sealed record EditProductionRequest(string Name, string? Description = null);
