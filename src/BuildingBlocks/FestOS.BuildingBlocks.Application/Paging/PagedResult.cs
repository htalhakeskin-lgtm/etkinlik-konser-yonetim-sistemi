namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>One numbered page and the total row count, so the table can show every page number (api §6.1).</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
