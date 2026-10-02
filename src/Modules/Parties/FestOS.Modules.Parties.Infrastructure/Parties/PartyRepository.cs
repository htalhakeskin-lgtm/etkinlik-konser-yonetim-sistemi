using FestOS.Modules.Parties.Application.Parties;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

internal sealed class PartyRepository(PartiesDbContext context) : IPartyRepository
{
    public Task<Party?> FindAsync(PartyId id, CancellationToken cancellationToken) =>
        context
            .Parties.Include(party => party.ContactPoints)
            .Include(party => party.ContactPersons)
            .Include(party => party.Representations)
            .AsSplitQuery()
            .SingleOrDefaultAsync(party => party.Id == id, cancellationToken);

    public Task<bool> RepresentsAnyArtistAsync(PartyId agencyId, CancellationToken cancellationToken) =>
        context.Parties.AnyAsync(
            party => party.Representations.Any(representation => representation.AgencyId == agencyId),
            cancellationToken
        );

    public void Add(Party party) => context.Parties.Add(party);
}
