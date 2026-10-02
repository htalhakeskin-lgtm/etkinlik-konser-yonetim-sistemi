using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
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

/// <summary>The parties (parties §8).</summary>
internal static class PartyEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/parties", ListPartiesAsync)
            .RequirePermission(PartiesPermissions.ViewParties)
            .WithName("ListParties")
            .WithSummary("Lists the parties, searched and filtered, one page at a time.");
        endpoints
            .MapGet("/parties/{partyId:guid}", GetPartyAsync)
            .RequirePermission(PartiesPermissions.ViewParties)
            .WithName("GetParty")
            .WithSummary("Gets a party with its contact points.");
        endpoints
            .MapPost("/parties", CreatePartyAsync)
            .RequirePermission(PartiesPermissions.CreateParties)
            .WithName("CreateParty")
            .WithSummary("Creates a person or organization with its roles and contact points.");
        endpoints
            .MapPut("/parties/{partyId:guid}", EditPartyAsync)
            .RequirePermission(PartiesPermissions.EditParties)
            .RequiresVersion()
            .WithName("EditParty")
            .WithSummary("Changes a party's names, roles and contact points.");
        endpoints
            .MapPost("/parties/{partyId:guid}/deactivate", DeactivatePartyAsync)
            .RequirePermission(PartiesPermissions.DeactivateParties)
            .RequiresVersion()
            .WithName("DeactivateParty")
            .WithSummary("Takes a party out of new selections.");
        endpoints
            .MapPost("/parties/{partyId:guid}/activate", ActivatePartyAsync)
            .RequirePermission(PartiesPermissions.DeactivateParties)
            .RequiresVersion()
            .WithName("ActivateParty")
            .WithSummary("Activates a deactivated party again.");
    }

    private static async Task<Ok<PagedResult<PartyListItem>>> ListPartiesAsync(
        [AsParameters] ListPartiesRequest request,
        IQueryHandler<ListPartiesQuery, PagedResult<PartyListItem>> listParties,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await listParties.HandleAsync(
                new ListPartiesQuery(
                    request.Q,
                    request.Role?.Value,
                    request.Kind?.Value,
                    request.Status?.Value ?? PartyStatusFilter.Active,
                    request.Sort,
                    new PageRequest(request.Page ?? 1, request.PageSize ?? PageRequest.DefaultPageSize)
                ),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<PartyDetails>>> GetPartyAsync(
        Guid partyId,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    ) => await CurrentAsync(PartyId.From(partyId), getParty, cancellationToken);

    private static async Task<Created<PartyDetails>> CreatePartyAsync(
        PartyRequest request,
        ICommandHandler<CreatePartyCommand, PartyId> createParty,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        PartyId id = await createParty.HandleAsync(
            new CreatePartyCommand(
                request.Kind ?? throw RequiredKind(),
                request.Name,
                request.FirstName,
                request.LastName,
                request.LegalName,
                request.Roles,
                request.ContactPointDetails()
            ),
            cancellationToken
        );
        PartyDetails party = await getParty.HandleAsync(new GetPartyQuery(id), cancellationToken);
        return TypedResults.Created($"/api/v1/parties/{id.Value}", party);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> EditPartyAsync(
        Guid partyId,
        PartyRequest request,
        ICommandHandler<EditPartyCommand, bool> editParty,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await editParty.HandleAsync(
            new EditPartyCommand(
                id,
                request.Name,
                request.FirstName,
                request.LastName,
                request.LegalName,
                request.Roles,
                request.ContactPointDetails()
            ),
            cancellationToken
        );
        return await CurrentAsync(id, getParty, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> DeactivatePartyAsync(
        Guid partyId,
        ICommandHandler<DeactivatePartyCommand, bool> deactivateParty,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await deactivateParty.HandleAsync(new DeactivatePartyCommand(id), cancellationToken);
        return await CurrentAsync(id, getParty, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<PartyDetails>>> ActivatePartyAsync(
        Guid partyId,
        ICommandHandler<ActivatePartyCommand, bool> activateParty,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        var id = PartyId.From(partyId);
        await activateParty.HandleAsync(new ActivatePartyCommand(id), cancellationToken);
        return await CurrentAsync(id, getParty, cancellationToken);
    }

    // After a change the answer is the party as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<PartyDetails>>> CurrentAsync(
        PartyId id,
        IQueryHandler<GetPartyQuery, PartyDetails> getParty,
        CancellationToken cancellationToken
    )
    {
        PartyDetails party = await getParty.HandleAsync(new GetPartyQuery(id), cancellationToken);
        return TypedResults.Ok(party).WithVersion(party.Version);
    }

    private static ValidationFailedException RequiredKind() =>
        new([
            new ValidationError("Kind", "NotEmptyValidator", new Dictionary<string, object?>(StringComparer.Ordinal)),
        ]);
}
