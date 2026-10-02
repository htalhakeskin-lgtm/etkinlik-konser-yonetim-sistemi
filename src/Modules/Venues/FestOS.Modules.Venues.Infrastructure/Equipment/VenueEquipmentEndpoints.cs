using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Venues.Application.Equipment;
using FestOS.Modules.Venues.Contracts;
using FestOS.Modules.Venues.Domain.Venues;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Venues.Infrastructure.Equipment;

/// <summary>
/// A venue's equipment (venues §6). Every change goes on the venue's version and answers with the venue's
/// equipment and its new version (venues VN-04).
/// </summary>
internal static class VenueEquipmentEndpoints
{
    private const string Lines = "/venues/{venueId:guid}/equipment";
    private const string Line = Lines + "/{lineId:guid}";
    private const string Periods = Line + "/unavailabilities";

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(Lines, ListAsync)
            .RequirePermission(VenuesPermissions.ViewVenues)
            .WithName("ListVenueEquipment")
            .WithSummary("Lists a venue's equipment with its unavailability periods.");
        endpoints
            .MapGet(Lines + "/usable", UsableAsync)
            .RequirePermission(VenuesPermissions.ViewVenues)
            .WithName("ListUsableVenueEquipment")
            .WithSummary("Tells how much of each line can be used on the given days.");
        endpoints
            .MapPost(Lines, AddAsync)
            .RequirePermission(VenuesPermissions.EditEquipment)
            .RequiresVersion()
            .WithName("AddVenueEquipment")
            .WithSummary("Adds a line to a venue's equipment.");
        endpoints
            .MapPut(Line, EditAsync)
            .RequirePermission(VenuesPermissions.EditEquipment)
            .RequiresVersion()
            .WithName("EditVenueEquipment")
            .WithSummary("Changes an equipment line.");
        endpoints
            .MapDelete(Line, RemoveAsync)
            .RequirePermission(VenuesPermissions.EditEquipment)
            .RequiresVersion()
            .WithName("RemoveVenueEquipment")
            .WithSummary("Removes a line entered by mistake.");
        endpoints
            .MapPost(Periods, AddPeriodAsync)
            .RequirePermission(VenuesPermissions.EditEquipment)
            .RequiresVersion()
            .WithName("AddVenueEquipmentUnavailability")
            .WithSummary("Records days when part of a line cannot be used.");
        endpoints
            .MapPut(Periods + "/{periodId:guid}", EditPeriodAsync)
            .RequirePermission(VenuesPermissions.EditEquipment)
            .RequiresVersion()
            .WithName("EditVenueEquipmentUnavailability")
            .WithSummary("Changes an unavailability period.");
        endpoints
            .MapDelete(Periods + "/{periodId:guid}", RemovePeriodAsync)
            .RequirePermission(VenuesPermissions.EditEquipment)
            .RequiresVersion()
            .WithName("RemoveVenueEquipmentUnavailability")
            .WithSummary("Removes an unavailability period.");
    }

    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> ListAsync(
        Guid venueId,
        [AsParameters] ListVenueEquipmentRequest request,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        VenueEquipmentList equipment = await list.HandleAsync(
            new ListVenueEquipmentQuery(VenueId.From(venueId), request.Status?.Value ?? EquipmentStatusFilter.Current),
            cancellationToken
        );
        return TypedResults.Ok(equipment).WithVersion(equipment.Version);
    }

    private static async Task<Ok<IReadOnlyList<UsableVenueEquipmentItem>>> UsableAsync(
        Guid venueId,
        [AsParameters] UsableVenueEquipmentRequest request,
        IQueryHandler<ListUsableVenueEquipmentQuery, IReadOnlyList<UsableVenueEquipmentItem>> usable,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await usable.HandleAsync(
                new ListUsableVenueEquipmentQuery(VenueId.From(venueId), request.From, request.To),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> AddAsync(
        Guid venueId,
        VenueEquipmentRequest request,
        ICommandHandler<AddVenueEquipmentCommand, VenueEquipmentId> add,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await add.HandleAsync(new AddVenueEquipmentCommand(id, request.Details()), cancellationToken);
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> EditAsync(
        Guid venueId,
        Guid lineId,
        VenueEquipmentRequest request,
        ICommandHandler<EditVenueEquipmentCommand, bool> edit,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await edit.HandleAsync(
            new EditVenueEquipmentCommand(id, VenueEquipmentId.From(lineId), request.Details()),
            cancellationToken
        );
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> RemoveAsync(
        Guid venueId,
        Guid lineId,
        ICommandHandler<RemoveVenueEquipmentCommand, bool> remove,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await remove.HandleAsync(new RemoveVenueEquipmentCommand(id, VenueEquipmentId.From(lineId)), cancellationToken);
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> AddPeriodAsync(
        Guid venueId,
        Guid lineId,
        UnavailabilityRequest request,
        ICommandHandler<AddVenueEquipmentUnavailabilityCommand, VenueEquipmentUnavailabilityId> add,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await add.HandleAsync(
            new AddVenueEquipmentUnavailabilityCommand(id, VenueEquipmentId.From(lineId), request.Details()),
            cancellationToken
        );
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> EditPeriodAsync(
        Guid venueId,
        Guid lineId,
        Guid periodId,
        UnavailabilityRequest request,
        ICommandHandler<EditVenueEquipmentUnavailabilityCommand, bool> edit,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await edit.HandleAsync(
            new EditVenueEquipmentUnavailabilityCommand(
                id,
                VenueEquipmentId.From(lineId),
                VenueEquipmentUnavailabilityId.From(periodId),
                request.Details()
            ),
            cancellationToken
        );
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> RemovePeriodAsync(
        Guid venueId,
        Guid lineId,
        Guid periodId,
        ICommandHandler<RemoveVenueEquipmentUnavailabilityCommand, bool> remove,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        var id = VenueId.From(venueId);
        await remove.HandleAsync(
            new RemoveVenueEquipmentUnavailabilityCommand(
                id,
                VenueEquipmentId.From(lineId),
                VenueEquipmentUnavailabilityId.From(periodId)
            ),
            cancellationToken
        );
        return await CurrentAsync(id, list, cancellationToken);
    }

    // After a change the answer is the venue's equipment as saved, with the venue's new version (api §9).
    private static async Task<VersionedResult<Ok<VenueEquipmentList>>> CurrentAsync(
        VenueId id,
        IQueryHandler<ListVenueEquipmentQuery, VenueEquipmentList> list,
        CancellationToken cancellationToken
    )
    {
        VenueEquipmentList equipment = await list.HandleAsync(
            new ListVenueEquipmentQuery(id, EquipmentStatusFilter.All),
            cancellationToken
        );
        return TypedResults.Ok(equipment).WithVersion(equipment.Version);
    }
}
