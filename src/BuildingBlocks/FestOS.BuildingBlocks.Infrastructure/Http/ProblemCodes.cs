namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>The problem types and technical codes of api §8 that are not in <c>ErrorCodes</c>.</summary>
internal static class ProblemCodes
{
    public const string TypePrefix = "urn:festos:problem:";

    public const string MalformedRequest = "malformedRequest";

    public const string InternalError = "internalError";

    public const string Unauthorized = "unauthorized";

    public const string Forbidden = "forbidden";

    public const string MethodNotAllowed = "methodNotAllowed";

    public const string VersionRequired = "versionRequired";

    public const string IdempotencyKeyMissing = "idempotencyKeyMissing";

    public const string CsrfRejected = "csrfRejected";
}
