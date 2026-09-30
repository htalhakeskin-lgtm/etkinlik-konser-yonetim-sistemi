namespace FestOS.BuildingBlocks.Infrastructure.Idempotency;

/// <summary>
/// The request's idempotency key and fingerprint, set by the <c>Idempotency-Key</c> filter (api §10). The
/// first command of the request claims them: its unit of work stores the key with the result, or returns
/// the result stored by an earlier request with the same key. One per scope.
/// </summary>
public sealed class IdempotencyRequest
{
    private bool _claimed;

    /// <summary>The key, or <see langword="null"/> when the request came with none.</summary>
    public Guid? Key { get; private set; }

    /// <summary>The SHA-256 of the request's method, address and body, as lowercase hex.</summary>
    public string? Fingerprint { get; private set; }

    /// <summary>Whether the command's result was stored by an earlier request with the same key.</summary>
    public bool Replayed { get; private set; }

    /// <summary>Records the request's key and fingerprint; set once, by the filter.</summary>
    public void Set(Guid key, string fingerprint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        if (fingerprint.Length != IdempotencyModel.FingerprintLength)
        {
            throw new ArgumentException("The fingerprint is a SHA-256 in hex.", nameof(fingerprint));
        }

        if (Key is not null)
        {
            throw new InvalidOperationException("The idempotency key of this request is already set.");
        }

        Key = key;
        Fingerprint = fingerprint;
    }

    // Only the first command of a request stores the key; later ones run as usual.
    internal bool TryClaim(out Guid key, out string fingerprint)
    {
        key = Key.GetValueOrDefault();
        fingerprint = Fingerprint ?? string.Empty;
        if (Key is null || _claimed)
        {
            return false;
        }

        _claimed = true;
        return true;
    }

    internal void MarkReplayed() => Replayed = true;
}
