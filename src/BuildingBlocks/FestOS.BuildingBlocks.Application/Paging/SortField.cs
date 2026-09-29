namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>One field of a sort order.</summary>
/// <param name="Name">The field name as the API spells it, e.g. <c>startsAt</c>.</param>
/// <param name="Descending">Whether the field sorts from high to low.</param>
public sealed record SortField(string Name, bool Descending);
