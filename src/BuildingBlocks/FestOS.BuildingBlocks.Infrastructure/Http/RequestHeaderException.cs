namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// A request header the endpoint requires is missing or unreadable, e.g. <c>If-Match</c> (api §9). Thrown
/// by endpoint filters; the exception handler writes it as Problem Details with the given status and code.
/// </summary>
internal sealed class RequestHeaderException(int statusCode, string type, string title, string code, string message)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    /// <summary>The last part of the problem type, e.g. <c>version-required</c>.</summary>
    public string Type { get; } = type;

    public string Title { get; } = title;

    public string Code { get; } = code;
}
