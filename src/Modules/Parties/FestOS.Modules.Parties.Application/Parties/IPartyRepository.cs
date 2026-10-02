using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>Loads and adds parties for commands (identity ID-01); the unit of work saves them.</summary>
public interface IPartyRepository
{
    /// <summary>The party with its contact points, contact persons and representations, or <see langword="null"/>.</summary>
    Task<Party?> FindAsync(PartyId id, CancellationToken cancellationToken);

    /// <summary>Whether the agency represents any artist (BR-PTY-004).</summary>
    Task<bool> RepresentsAnyArtistAsync(PartyId agencyId, CancellationToken cancellationToken);

    /// <summary>Adds a new party.</summary>
    void Add(Party party);
}
