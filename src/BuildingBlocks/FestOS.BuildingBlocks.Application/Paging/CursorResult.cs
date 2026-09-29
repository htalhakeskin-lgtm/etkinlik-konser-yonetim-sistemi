namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>One slice of a growing list; <see cref="NextCursor"/> is <see langword="null"/> on the last slice (api §6.1).</summary>
public sealed record CursorResult<T>(IReadOnlyList<T> Items, string? NextCursor);
