using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Venues.Application.Venues;
using FestOS.Modules.Venues.Contracts;
using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Venues.Infrastructure.Venues;

/// <summary>The venues (venues §6).</summary>
internal static class VenueEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/venues", ListAsync)
            .RequirePermission(VenuesPermissions.ViewVenues)
            .WithName("ListVenues")
            .WithSummary("Lists the venues, searched and filtered, one page at a time.");
        endpoints
            .MapGet("/venues/{venueId:guid}", GetAsync)
            .RequirePermission(VenuesPermissions.ViewVenues)
            .WithName("GetVenue")
            .WithSummary("Gets a venue with its operator's name.");
        endpoints
            .MapPost("/venues", CreateAsync)
            .RequirePermission(VenuesPermissions.CreateVenues)
            .WithName("CreateVenue")
            .WithSummary("Records a venue with its technical details.");
        endpoints
            .MapPut("/venues/{venueId:guid}", EditAsync)
            .RequirePermission(VenuesPermissions.EditVenues)
            .RequiresVersion()
            .WithName("EditVenue")
            .WithSummary("Changes a venue's details.");
        endpoints
            .MapPost("/venues/{venueId:guid}/deactivate", DeactivateAsync)
            .RequirePermission(VenuesPermissions.DeactivateVenues)
            .RequiresVersion()
            .WithName("DeactivateVenue")
            .WithSummary("Takes a venue out of new selections.");
        endpoints
            .MapPost("/venues/{venueId:guid}/activate", ActivateAsync)
            .RequirePermission(VenuesPermissions.DeactivateVenues)
            .RequiresVersion()
            .WithName("ActivateVenue")
            .WithSummary("Activates a deactivated venue again.");
    }

    private static async Task<Ok<PagedResult<VenueListItem>>> ListAsync(
        [AsParameters] ListVenuesRequest request,
        IQueryHandler<ListVenuesQuery, PagedResult<VenueListItem>> list,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await list.HandleAsync(
                new ListVenuesQuery(
                    request.Q,
                    request.City,
                    request.Status?.Value ?? VenueStatusFilter.Active,
                    request.Sort,
                    new PageRequest(request.Page ?? 1, request.PageSize ?? PageRequest.DefaultPageSize)
                ),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<VenueDetails>>> GetAsync(
        Guid venueId,
        IQueryHandler<GetVenueQuery, VenueDetails> get,
        CancellationToken cancellationToken
    ) => await CurrentAsync(VenueId.From(venueId), get, cancellationToken);

    private static async Task<Created<VenueDetails>> CreateAsync(
        VenueRequest request,
        ICommandHandler<CreateVenueCommand, VenueId> create,
        IQueryHandler<GetVenueQuery, VenueDetails> get,
        CancellationToken cancellationToken
    )
    {
        VenueId id = await create.HandleAsync(new CreateVenueCommand(request.Description()), cancellationToken);
        VenueDetails venue = await get.HandleAsync(new GetVenueQuery(id), cancellationToken);
        return TypedResults.Created($"/api/v1/venues/{id.Value}", venue);
    }

    private static async Task<VersionedResult<Ok<VenueDetails>>> EditAsync(
        Guid venueId,
        VenueRequest request,
        ICommandHandler<EditVenueCommand, bool> edit,
        IQueryHandler<GetVenueQuery, VenueDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await edit.HandleAsync(new EditVenueCommand(id, request.Description()), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<VenueDetails>>> DeactivateAsync(
        Guid venueId,
        ICommandHandler<DeactivateVenueCommand, bool> deactivate,
        IQueryHandler<GetVenueQuery, VenueDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await deactivate.HandleAsync(new DeactivateVenueCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<VenueDetails>>> ActivateAsync(
        Guid venueId,
        ICommandHandler<ActivateVenueCommand, bool> activate,
        IQueryHandler<GetVenueQuery, VenueDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await activate.HandleAsync(new ActivateVenueCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    // After a change the answer is the venue as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<VenueDetails>>> CurrentAsync(
        VenueId id,
        IQueryHandler<GetVenueQuery, VenueDetails> get,
        CancellationToken cancellationToken
    )
    {
        VenueDetails venue = await get.HandleAsync(new GetVenueQuery(id), cancellationToken);
        return TypedResults.Ok(venue).WithVersion(venue.Version);
    }
}
