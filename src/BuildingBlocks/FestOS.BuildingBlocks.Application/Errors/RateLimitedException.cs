namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>Too many requests of one kind; the API answers 429 with <c>Retry-After</c> (api §11).</summary>
public sealed class RateLimitedException(TimeSpan retryAfter) : Exception("Too many requests; try again later.")
{
    /// <summary>How long the client waits before trying again.</summary>
    public TimeSpan RetryAfter { get; } = retryAfter;
}
