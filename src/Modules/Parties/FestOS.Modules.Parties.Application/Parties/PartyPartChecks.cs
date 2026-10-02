using FestOS.BuildingBlocks.Application.Errors;
using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>A contact person or representation named in the address belongs to the party, or the answer is 404.</summary>
internal static class PartyPartChecks
{
    public static void EnsureHasContactPerson(this Party party, OrganizationContactId id)
    {
        if (!party.ContactPersons.Any(contact => contact.Id == id))
        {
            throw new NotFoundException("OrganizationContact", id.Value);
        }
    }

    public static void EnsureHasRepresentation(this Party party, ArtistRepresentationId id)
    {
        if (!party.Representations.Any(representation => representation.Id == id))
        {
            throw new NotFoundException("ArtistRepresentation", id.Value);
        }
    }
}
