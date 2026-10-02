using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Riders.Application.Productions;
using FestOS.Modules.Riders.Contracts;
using FestOS.Modules.Riders.Domain.Productions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Riders.Infrastructure.Productions;

/// <summary>The productions (riders §6).</summary>
internal static class ProductionEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/productions", ListAsync)
            .RequirePermission(RidersPermissions.ViewProductions)
            .WithName("ListProductions")
            .WithSummary("Lists the productions, searched and filtered, one page at a time.");
        endpoints
            .MapGet("/productions/{productionId:guid}", GetAsync)
            .RequirePermission(RidersPermissions.ViewProductions)
            .WithName("GetProduction")
            .WithSummary("Gets a production with its artist's name and its rider.");
        endpoints
            .MapPost("/productions", CreateAsync)
            .RequirePermission(RidersPermissions.CreateProductions)
            .WithName("CreateProduction")
            .WithSummary("Records an artist's production and opens its empty rider.");
        endpoints
            .MapPut("/productions/{productionId:guid}", EditAsync)
            .RequirePermission(RidersPermissions.EditProductions)
            .RequiresVersion()
            .WithName("EditProduction")
            .WithSummary("Changes a production's name and description.");
        endpoints
            .MapPost("/productions/{productionId:guid}/deactivate", DeactivateAsync)
            .RequirePermission(RidersPermissions.DeactivateProductions)
            .RequiresVersion()
            .WithName("DeactivateProduction")
            .WithSummary("Takes a production out of new selections.");
        endpoints
            .MapPost("/productions/{productionId:guid}/activate", ActivateAsync)
            .RequirePermission(RidersPermissions.DeactivateProductions)
            .RequiresVersion()
            .WithName("ActivateProduction")
            .WithSummary("Activates a deactivated production again.");
    }

    private static async Task<Ok<PagedResult<ProductionListItem>>> ListAsync(
        [AsParameters] ListProductionsRequest request,
        IQueryHandler<ListProductionsQuery, PagedResult<ProductionListItem>> list,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await list.HandleAsync(
                new ListProductionsQuery(
                    request.ArtistId,
                    request.Q,
                    request.Status?.Value ?? ProductionStatusFilter.Active,
                    request.Sort,
                    new PageRequest(request.Page ?? 1, request.PageSize ?? PageRequest.DefaultPageSize)
                ),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<ProductionDetails>>> GetAsync(
        Guid productionId,
        IQueryHandler<GetProductionQuery, ProductionDetails> get,
        CancellationToken cancellationToken
    ) => await CurrentAsync(ProductionId.From(productionId), get, cancellationToken);

    private static async Task<Created<ProductionDetails>> CreateAsync(
        CreateProductionRequest request,
        ICommandHandler<CreateProductionCommand, ProductionId> create,
        IQueryHandler<GetProductionQuery, ProductionDetails> get,
        CancellationToken cancellationToken
    )
    {
        ProductionId id = await create.HandleAsync(
            new CreateProductionCommand(request.ArtistPartyId, request.Name, request.Description),
            cancellationToken
        );
        ProductionDetails production = await get.HandleAsync(new GetProductionQuery(id), cancellationToken);
        return TypedResults.Created($"/api/v1/productions/{id.Value}", production);
    }

    private static async Task<VersionedResult<Ok<ProductionDetails>>> EditAsync(
        Guid productionId,
        EditProductionRequest request,
        ICommandHandler<EditProductionCommand, bool> edit,
        IQueryHandler<GetProductionQuery, ProductionDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = ProductionId.From(productionId);
        await edit.HandleAsync(new EditProductionCommand(id, request.Name, request.Description), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<ProductionDetails>>> DeactivateAsync(
        Guid productionId,
        ICommandHandler<DeactivateProductionCommand, bool> deactivate,
        IQueryHandler<GetProductionQuery, ProductionDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = ProductionId.From(productionId);
        await deactivate.HandleAsync(new DeactivateProductionCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<ProductionDetails>>> ActivateAsync(
        Guid productionId,
        ICommandHandler<ActivateProductionCommand, bool> activate,
        IQueryHandler<GetProductionQuery, ProductionDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = ProductionId.From(productionId);
        await activate.HandleAsync(new ActivateProductionCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    // After a change the answer is the production as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<ProductionDetails>>> CurrentAsync(
        ProductionId id,
        IQueryHandler<GetProductionQuery, ProductionDetails> get,
        CancellationToken cancellationToken
    )
    {
        ProductionDetails production = await get.HandleAsync(new GetProductionQuery(id), cancellationToken);
        return TypedResults.Ok(production).WithVersion(production.Version);
    }
}
