namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>
/// What an integration event changed, for the <c>resourceChanged</c> notification (api §13): the resource
/// as it is named in addresses, the record, its new version when the event carries it, and the groups to
/// tell (naming §8.3).
/// </summary>
/// <param name="Resource">The resource's address segment, e.g. <c>events</c>.</param>
/// <param name="Id">The changed record.</param>
/// <param name="Version">
/// The record's version after the change, or <see langword="null"/> when the event does not carry it; the
/// client then always reads again.
/// </param>
/// <param name="Groups">The groups to notify, e.g. <c>events</c> and <c>events:{id}</c>.</param>
public sealed record ResourceChange(string Resource, Guid Id, int? Version, IReadOnlyList<string> Groups);
