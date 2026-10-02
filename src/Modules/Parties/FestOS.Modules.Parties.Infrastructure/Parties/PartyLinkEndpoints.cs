using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Parties.Application.Parties;
using FestOS.Modules.Parties.Contracts;
using FestOS.Modules.Parties.Domain.Parties;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Parties.Infrastructure.Parties;

/// <summary>
/// The ties between parties (parties §8): an organization's contact persons and an artist's agencies. Each
/// changes the party that holds the tie, on its version, and answers with that party.
/// </summary>
internal static class PartyLinkEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost("/parties/{partyId:guid}/contact-persons", AddContactPersonAsync)
            .RequirePermission(PartiesPermissions.EditParties)
            .RequiresVersion()
            .WithName("AddContactPerson")
            .WithSummary("Ties a person to an organization as its contact person.");
        endpoints
            .MapPut("/parties/{partyId:guid}/contact-persons/{contactId:guid}", RetitleContactPersonAsync)
            .RequirePermission(PartiesPermissions.EditParties)
            .RequiresVersion()
            .WithName("EditContactPerson")
            .WithSummary("Changes a contact person's job title.");
        endpoints
            .MapDelete("/parties/{partyId:guid}/contact-persons/{contactId:guid}", RemoveContactPersonAsync)
            .RequirePermission(PartiesPermissions.EditParties)
            .RequiresVersion()
            .WithName("RemoveContactPerson")
            .WithSummary("Unties a contact person from an organization; the person stays.");
        endpoints
            .MapPost("/parties/{partyId:guid}/representations", AddRepresentationAsync)
            .RequirePermission(PartiesPermissions.EditParties)
            .RequiresVersion()
            .WithName("AddRepresentation")
            .WithSummary("Records that an agency represents an artist.");
        endpoints
            .MapPut("/parties/{partyId:guid}/representations/{representationId:guid}", DescribeRepresentationAsync)
            .RequirePermission(PartiesPermissions.EditParties)
            .RequiresVersion()
            .WithName("EditRepresentation")
            .WithSummary("Changes what a representation covers.");
        endpoints
            .MapDelete("/parties/{partyId:guid}/representations/{representationId:guid}", RemoveRepresentationAsync)
            .RequirePermission(PartiesPermissions.EditParties)
            .RequiresVersion()
            .WithName("RemoveRepresentation")
            .WithSummary("Ends a representation.");
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> AddContactPersonAsync(
        Guid partyId,
        AddContactPersonRequest request,
        ICommandHandler<AddContactPersonCommand, OrganizationContactId> addContactPerson,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await addContactPerson.HandleAsync(
            new AddContactPersonCommand(id, PartyId.From(request.PersonId), request.Title),
            cancellationToken
        );
        return await PartyEndpoints.CurrentAsync(id, getParty, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> RetitleContactPersonAsync(
        Guid partyId,
        Guid contactId,
        ContactPersonTitleRequest request,
        ICommandHandler<RetitleContactPersonCommand, bool> retitleContactPerson,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await retitleContactPerson.HandleAsync(
            new RetitleContactPersonCommand(id, OrganizationContactId.From(contactId), request.Title),
            cancellationToken
        );
        return await PartyEndpoints.CurrentAsync(id, getParty, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> RemoveContactPersonAsync(
        Guid partyId,
        Guid contactId,
        ICommandHandler<RemoveContactPersonCommand, bool> removeContactPerson,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await removeContactPerson.HandleAsync(
            new RemoveContactPersonCommand(id, OrganizationContactId.From(contactId)),
            cancellationToken
        );
        return await PartyEndpoints.CurrentAsync(id, getParty, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> AddRepresentationAsync(
        Guid partyId,
        AddRepresentationRequest request,
        ICommandHandler<AddRepresentationCommand, ArtistRepresentationId> addRepresentation,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await addRepresentation.HandleAsync(
            new AddRepresentationCommand(id, PartyId.From(request.AgencyId), request.Description),
            cancellationToken
        );
        return await PartyEndpoints.CurrentAsync(id, getParty, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> DescribeRepresentationAsync(
        Guid partyId,
        Guid representationId,
        RepresentationDescriptionRequest request,
        ICommandHandler<DescribeRepresentationCommand, bool> describeRepresentation,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await describeRepresentation.HandleAsync(
            new DescribeRepresentationCommand(id, ArtistRepresentationId.From(representationId), request.Description),
            cancellationToken
        );
        return await PartyEndpoints.CurrentAsync(id, getParty, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> RemoveRepresentationAsync(
        Guid partyId,
        Guid representationId,
        ICommandHandler<RemoveRepresentationCommand, bool> removeRepresentation,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await removeRepresentation.HandleAsync(
            new RemoveRepresentationCommand(id, ArtistRepresentationId.From(representationId)),
            cancellationToken
        );
        return await PartyEndpoints.CurrentAsync(id, getParty, cancellationToken);
    }
}
