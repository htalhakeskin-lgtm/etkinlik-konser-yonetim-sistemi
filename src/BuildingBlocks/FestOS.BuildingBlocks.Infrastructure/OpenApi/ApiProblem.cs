using FestOS.BuildingBlocks.Infrastructure.Http;

namespace FestOS.BuildingBlocks.Infrastructure.OpenApi;

/// <summary>
/// The shape of every error response, for the OpenAPI document (api §8): RFC 9457 Problem Details with
/// the project's <c>code</c>, <c>params</c>, <c>errors</c> and <c>traceId</c>. Responses are written by
/// the exception handler, never from this type.
/// </summary>
internal sealed class ApiProblem
{
    public required string Type { get; init; }

    public required string Title { get; init; }

    public required int Status { get; init; }

    public required string Detail { get; init; }

    public required string Instance { get; init; }

    public required string Code { get; init; }

    public required string TraceId { get; init; }

    /// <summary>Only in a business rule violation that has values for its message.</summary>
    public IReadOnlyDictionary<string, object?> Params { get; init; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>Only in a validation problem.</summary>
    public IReadOnlyList<ValidationProblemError> Errors { get; init; } = [];
}
