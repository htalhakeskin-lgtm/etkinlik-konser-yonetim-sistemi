using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Catalog.Application.Kits;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Catalog.Domain.Kits;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Catalog.Infrastructure.Kits;

/// <summary>The kits (catalog §6).</summary>
internal static class KitEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/kits", ListAsync)
            .RequirePermission(CatalogPermissions.ViewKits)
            .WithName("ListKits")
            .WithSummary("Lists the kits with their totals, one page at a time.");
        endpoints
            .MapGet("/kits/{kitId:guid}", GetAsync)
            .RequirePermission(CatalogPermissions.ViewKits)
            .WithName("GetKit")
            .WithSummary("Gets a kit with its lines, contents and totals.");
        endpoints
            .MapPost("/kits", CreateAsync)
            .RequirePermission(CatalogPermissions.CreateKits)
            .WithName("CreateKit")
            .WithSummary("Defines a kit.");
        endpoints
            .MapPut("/kits/{kitId:guid}", EditAsync)
            .RequirePermission(CatalogPermissions.EditKits)
            .RequiresVersion()
            .WithName("EditKit")
            .WithSummary("Renames a kit and sets its lines.");
        endpoints
            .MapPost("/kits/{kitId:guid}/deactivate", DeactivateAsync)
            .RequirePermission(CatalogPermissions.DeactivateKits)
            .RequiresVersion()
            .WithName("DeactivateKit")
            .WithSummary("Takes a kit out of new selections.");
        endpoints
            .MapPost("/kits/{kitId:guid}/activate", ActivateAsync)
            .RequirePermission(CatalogPermissions.DeactivateKits)
            .RequiresVersion()
            .WithName("ActivateKit")
            .WithSummary("Activates a deactivated kit again.");
    }

    private static async Task<Ok<PagedResult<KitListItem>>> ListAsync(
        [AsParameters] ListKitsRequest request,
        IQueryHandler<ListKitsQuery, PagedResult<KitListItem>> list,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await list.HandleAsync(
                new ListKitsQuery(
                    request.Q,
                    request.Status?.Value ?? KitStatusFilter.Active,
                    new PageRequest(request.Page ?? 1, request.PageSize ?? PageRequest.DefaultPageSize)
                ),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<KitDetails>>> GetAsync(
        Guid kitId,
        IQueryHandler<GetKitQuery, KitDetails> get,
        CancellationToken cancellationToken
    ) => await CurrentAsync(KitId.From(kitId), get, cancellationToken);

    private static async Task<Created<KitDetails>> CreateAsync(
        KitRequest request,
        ICommandHandler<CreateKitCommand, KitId> create,
        IQueryHandler<GetKitQuery, KitDetails> get,
        CancellationToken cancellationToken
    )
    {
        KitId id = await create.HandleAsync(
            new CreateKitCommand(request.Name, request.LineDetails()),
            cancellationToken
        );
        KitDetails kit = await get.HandleAsync(new GetKitQuery(id), cancellationToken);
        return TypedResults.Created($"/api/v1/kits/{id.Value}", kit);
    }

    private static async Task<VersionedResult<Ok<KitDetails>>> EditAsync(
        Guid kitId,
        KitRequest request,
        ICommandHandler<EditKitCommand, bool> edit,
        IQueryHandler<GetKitQuery, KitDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = KitId.From(kitId);
        await edit.HandleAsync(new EditKitCommand(id, request.Name, request.LineDetails()), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<KitDetails>>> DeactivateAsync(
        Guid kitId,
        ICommandHandler<DeactivateKitCommand, bool> deactivate,
        IQueryHandler<GetKitQuery, KitDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = KitId.From(kitId);
        await deactivate.HandleAsync(new DeactivateKitCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<KitDetails>>> ActivateAsync(
        Guid kitId,
        ICommandHandler<ActivateKitCommand, bool> activate,
        IQueryHandler<GetKitQuery, KitDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = KitId.From(kitId);
        await activate.HandleAsync(new ActivateKitCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    // After a change the answer is the kit as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<KitDetails>>> CurrentAsync(
        KitId id,
        IQueryHandler<GetKitQuery, KitDetails> get,
        CancellationToken cancellationToken
    )
    {
        KitDetails kit = await get.HandleAsync(new GetKitQuery(id), cancellationToken);
        return TypedResults.Ok(kit).WithVersion(kit.Version);
    }
}
