namespace FestOS.Modules.Parties.Contracts;

/// <summary>
/// What other modules may ask Parties synchronously (05 §5.3): the parties with these identifiers, to check a
/// selection (BR-PTY-004) or to show names (parties MD-02).
/// </summary>
public interface IPartyDirectory
{
    /// <summary>The parties among <paramref name="ids"/> that exist, by identifier.</summary>
    Task<IReadOnlyDictionary<Guid, PartySummary>> FindAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken
    );
}
