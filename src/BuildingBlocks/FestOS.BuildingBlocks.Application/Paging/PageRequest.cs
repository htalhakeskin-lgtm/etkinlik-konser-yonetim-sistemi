namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>
/// A numbered page of a list, <c>?page=2&amp;pageSize=25</c>, for office tables (api §6.1). Checked by
/// <see cref="PageRequestValidator"/>.
/// </summary>
/// <param name="Page">The page number, starting at 1.</param>
/// <param name="PageSize">Rows per page, at most <see cref="MaxPageSize"/>.</param>
public sealed record PageRequest(int Page = 1, int PageSize = PageRequest.DefaultPageSize)
{
    /// <summary>Rows per page when the request does not say.</summary>
    public const int DefaultPageSize = 25;

    /// <summary>The largest page a request may ask for.</summary>
    public const int MaxPageSize = 100;

    /// <summary>How many rows come before this page.</summary>
    public int Skip => (Page - 1) * PageSize;
}
