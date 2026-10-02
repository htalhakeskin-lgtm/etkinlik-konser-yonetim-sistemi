using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Riders.Application.Riders;
using FestOS.Modules.Riders.Contracts;
using FestOS.Modules.Riders.Domain.Riders;
using FestOS.Modules.Riders.Domain.RiderVersions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Riders.Infrastructure.Riders;

/// <summary>The riders' versions (riders §6).</summary>
internal static class RiderEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/riders/{riderId:guid}/versions", ListVersionsAsync)
            .RequirePermission(RidersPermissions.ViewRiders)
            .WithName("ListRiderVersions")
            .WithSummary("Lists a rider's versions, newest first, with the rider's version.");
        endpoints
            .MapGet("/rider-versions/{versionId:guid}", GetVersionAsync)
            .RequirePermission(RidersPermissions.ViewRiders)
            .WithName("GetRiderVersion")
            .WithSummary("Gets a rider version with its lines.");
        endpoints
            .MapPost("/riders/{riderId:guid}/versions", CreateVersionAsync)
            .RequirePermission(RidersPermissions.EditRiders)
            .RequiresVersion()
            .WithName("CreateRiderVersion")
            .WithSummary("Saves a rider's lines as its next version.");
    }

    private static async Task<VersionedResult<Ok<RiderVersionList>>> ListVersionsAsync(
        Guid riderId,
        IQueryHandler<ListRiderVersionsQuery, RiderVersionList> list,
        CancellationToken cancellationToken
    )
    {
        RiderVersionList versions = await list.HandleAsync(
            new ListRiderVersionsQuery(RiderId.From(riderId)),
            cancellationToken
        );
        return TypedResults.Ok(versions).WithVersion(versions.Version);
    }

    private static async Task<Ok<RiderVersionDetails>> GetVersionAsync(
        Guid versionId,
        IQueryHandler<GetRiderVersionQuery, RiderVersionDetails> get,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await get.HandleAsync(new GetRiderVersionQuery(RiderVersionId.From(versionId)), cancellationToken)
        );

    private static async Task<Created<RiderVersionDetails>> CreateVersionAsync(
        Guid riderId,
        CreateRiderVersionRequest request,
        ICommandHandler<CreateRiderVersionCommand, RiderVersionId> create,
        IQueryHandler<GetRiderVersionQuery, RiderVersionDetails> get,
        CancellationToken cancellationToken
    )
    {
        RiderVersionId id = await create.HandleAsync(
            new CreateRiderVersionCommand(
                RiderId.From(riderId),
                [.. (request.Lines ?? []).Select(line => line.Details())],
                request.Note
            ),
            cancellationToken
        );
        RiderVersionDetails version = await get.HandleAsync(new GetRiderVersionQuery(id), cancellationToken);
        return TypedResults.Created($"/api/v1/rider-versions/{id.Value}", version);
    }
}
