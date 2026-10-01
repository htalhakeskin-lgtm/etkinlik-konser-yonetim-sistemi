using FestOS.BuildingBlocks.Application.Paging;
using Microsoft.EntityFrameworkCore;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>Reads one numbered page of an ordered query (api §6.1).</summary>
public static class PagingExtensions
{
    /// <summary>The rows of the page and the total count; the query is already sorted.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> rows,
        PageRequest page,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(page);

        int totalCount = await rows.CountAsync(cancellationToken);
        List<T> items = await rows.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, page.Page, page.PageSize, totalCount);
    }
}
