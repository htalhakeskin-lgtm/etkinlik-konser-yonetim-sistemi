namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>One entry of a validation problem's <c>errors</c>.</summary>
internal sealed record ValidationProblemError(string Pointer, string Code, IReadOnlyDictionary<string, object?> Params);
