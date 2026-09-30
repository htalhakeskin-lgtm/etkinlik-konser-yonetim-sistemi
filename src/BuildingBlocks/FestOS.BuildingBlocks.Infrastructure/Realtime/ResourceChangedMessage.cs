namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>
/// The only message the server sends to clients (api §13). It carries no data, only what changed; the
/// client invalidates its queries and reads again through the API, where access is checked.
/// </summary>
public sealed record ResourceChangedMessage(string Resource, Guid Id, int? Version);
