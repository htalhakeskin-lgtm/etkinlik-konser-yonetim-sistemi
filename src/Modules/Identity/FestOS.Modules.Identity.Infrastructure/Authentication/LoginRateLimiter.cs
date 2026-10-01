using System.Net;
using System.Threading.RateLimiting;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

/// <summary>
/// Limits sign-in attempts per address and per email (identity §6, api §11), apart from the account lock.
/// The email is in the body, which the rate limiting middleware cannot read, so the endpoint asks here.
/// </summary>
internal sealed class LoginRateLimiter : IDisposable
{
    public const int PermitsPerAddress = 10;
    public const int PermitsPerEmail = 5;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private readonly PartitionedRateLimiter<(string Address, string Email)> _limiter =
        PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<(string Address, string Email), string>(key =>
                RateLimitPartition.GetFixedWindowLimiter(key.Address, _ => Options(PermitsPerAddress))
            ),
            PartitionedRateLimiter.Create<(string Address, string Email), string>(key =>
                RateLimitPartition.GetFixedWindowLimiter(key.Email, _ => Options(PermitsPerEmail))
            )
        );

    /// <summary>Takes a permit for an attempt; when there is none, says how long to wait.</summary>
    public bool TryAcquire(IPAddress? address, string normalizedEmail, out TimeSpan retryAfter)
    {
        using RateLimitLease lease = _limiter.AttemptAcquire((address?.ToString() ?? "unknown", normalizedEmail));
        retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan after) ? after : Window;
        return lease.IsAcquired;
    }

    public void Dispose() => _limiter.Dispose();

    private static FixedWindowRateLimiterOptions Options(int permits) =>
        new()
        {
            PermitLimit = permits,
            Window = Window,
            QueueLimit = 0,
            AutoReplenishment = true,
        };
}
