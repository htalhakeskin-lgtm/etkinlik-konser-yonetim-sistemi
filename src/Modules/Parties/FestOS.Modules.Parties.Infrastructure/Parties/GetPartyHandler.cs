using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.Modules.Parties.Application.Parties;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.EntityFrameworkCore;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

internal sealed class GetPartyHandler(PartiesDbContext context) : IQueryHandler<GetPartyQuery, PartyDetails>
{
    public async Task<PartyDetails> HandleAsync(GetPartyQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Party> parties = context.Parties.AsNoTracking();
        return await parties
                .AsSplitQuery()
                .Where(party => party.Id == query.Id)
                .Select(party => new PartyDetails(
                    party.Id,
                    party.Kind,
                    party.Name,
                    party.FirstName,
                    party.LastName,
                    party.LegalName,
                    party.Roles,
                    party
                        .ContactPoints.OrderBy(point => point.SortOrder)
                        .Select(point => new ContactPointItem(
                            point.Id,
                            point.Kind,
                            point.Value,
                            point.Label,
                            point.IsPrimary
                        ))
                        .ToList(),
                    party
                        .ContactPersons.OrderBy(contact => contact.SortOrder)
                        .Join(
                            parties,
                            contact => contact.PersonId,
                            person => person.Id,
                            (contact, person) =>
                                new ContactPersonItem(
                                    contact.Id,
                                    person.Id,
                                    person.Name,
                                    contact.Title,
                                    person
                                        .ContactPoints.Where(point =>
                                            point.Kind == ContactPointKind.Phone && point.IsPrimary
                                        )
                                        .Select(point => point.Value)
                                        .FirstOrDefault(),
                                    person
                                        .ContactPoints.Where(point =>
                                            point.Kind == ContactPointKind.Email && point.IsPrimary
                                        )
                                        .Select(point => point.Value)
                                        .FirstOrDefault(),
                                    person.DeactivatedAt == null
                                )
                        )
                        .ToList(),
                    party
                        .Representations.Join(
                            parties,
                            representation => representation.AgencyId,
                            agency => agency.Id,
                            (representation, agency) =>
                                new RepresentationItem(
                                    representation.Id,
                                    agency.Id,
                                    agency.Name,
                                    representation.Description,
                                    agency.DeactivatedAt == null
                                )
                        )
                        .ToList(),
                    parties
                        .SelectMany(
                            artist =>
                                artist.Representations.Where(representation => representation.AgencyId == party.Id),
                            (artist, representation) =>
                                new RepresentedArtistItem(
                                    artist.Id,
                                    artist.Name,
                                    representation.Description,
                                    artist.DeactivatedAt == null
                                )
                        )
                        .ToList(),
                    parties
                        .SelectMany(
                            organization => organization.ContactPersons.Where(contact => contact.PersonId == party.Id),
                            (organization, contact) =>
                                new EmployerItem(
                                    organization.Id,
                                    organization.Name,
                                    contact.Title,
                                    organization.DeactivatedAt == null
                                )
                        )
                        .ToList(),
                    party.DeactivatedAt,
                    party.CreatedAt,
                    party.UpdatedAt,
                    party.Version
                ))
                .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Party", query.Id.Value);
    }
}
