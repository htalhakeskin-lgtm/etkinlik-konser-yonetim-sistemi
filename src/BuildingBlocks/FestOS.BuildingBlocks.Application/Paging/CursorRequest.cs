namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>
/// The next slice of a growing list, <c>?after={cursor}&amp;limit=50</c>, for append-only records and
/// "load more" lists (api §6.1). Checked by <see cref="CursorRequestValidator"/>.
/// </summary>
/// <param name="After">The opaque cursor from the previous result; <see langword="null"/> for the first slice.</param>
/// <param name="Limit">Rows to return, at most <see cref="MaxLimit"/>.</param>
public sealed record CursorRequest(string? After = null, int Limit = CursorRequest.DefaultLimit)
{
    /// <summary>Rows per slice when the request does not say.</summary>
    public const int DefaultLimit = 50;

    /// <summary>The largest slice a request may ask for.</summary>
    public const int MaxLimit = 100;

    /// <summary>The longest cursor accepted; cursors are short encoded sort values.</summary>
    public const int MaxCursorLength = 512;
}
