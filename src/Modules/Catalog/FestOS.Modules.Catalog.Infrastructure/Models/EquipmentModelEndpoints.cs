using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Catalog.Application.Models;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Catalog.Domain.Categories;
using FestOS.Modules.Catalog.Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Catalog.Infrastructure.Models;

/// <summary>The models (catalog §6).</summary>
internal static class EquipmentModelEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/equipment-models", ListAsync)
            .RequirePermission(CatalogPermissions.ViewModels)
            .WithName("ListEquipmentModels")
            .WithSummary("Lists the models, searched and filtered, one page at a time.");
        endpoints
            .MapGet("/equipment-models/{modelId:guid}", GetAsync)
            .RequirePermission(CatalogPermissions.ViewModels)
            .WithName("GetEquipmentModel")
            .WithSummary("Gets a model.");
        endpoints
            .MapPost("/equipment-models", CreateAsync)
            .RequirePermission(CatalogPermissions.CreateModels)
            .WithName("CreateEquipmentModel")
            .WithSummary("Adds a model to the catalog.");
        endpoints
            .MapPut("/equipment-models/{modelId:guid}", EditAsync)
            .RequirePermission(CatalogPermissions.EditModels)
            .RequiresVersion()
            .WithName("EditEquipmentModel")
            .WithSummary("Changes a model; the tracking type only while it has no stock.");
        endpoints
            .MapPost("/equipment-models/{modelId:guid}/deactivate", DeactivateAsync)
            .RequirePermission(CatalogPermissions.DeactivateModels)
            .RequiresVersion()
            .WithName("DeactivateEquipmentModel")
            .WithSummary("Takes a model out of new selections.");
        endpoints
            .MapPost("/equipment-models/{modelId:guid}/activate", ActivateAsync)
            .RequirePermission(CatalogPermissions.DeactivateModels)
            .RequiresVersion()
            .WithName("ActivateEquipmentModel")
            .WithSummary("Activates a deactivated model in an active category.");
    }

    private static async Task<Ok<PagedResult<EquipmentModelListItem>>> ListAsync(
        [AsParameters] ListEquipmentModelsRequest request,
        IQueryHandler<ListEquipmentModelsQuery, PagedResult<EquipmentModelListItem>> list,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await list.HandleAsync(
                new ListEquipmentModelsQuery(
                    request.Q,
                    request.CategoryId is { } category ? EquipmentCategoryId.From(category) : null,
                    request.TrackingType?.Value,
                    request.Status?.Value ?? ModelStatusFilter.Active,
                    request.Sort,
                    new PageRequest(request.Page ?? 1, request.PageSize ?? PageRequest.DefaultPageSize)
                ),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<EquipmentModelDetails>>> GetAsync(
        Guid modelId,
        IQueryHandler<GetEquipmentModelQuery, EquipmentModelDetails> get,
        CancellationToken cancellationToken
    ) => await CurrentAsync(EquipmentModelId.From(modelId), get, cancellationToken);

    private static async Task<Created<EquipmentModelDetails>> CreateAsync(
        EquipmentModelRequest request,
        ICommandHandler<CreateEquipmentModelCommand, EquipmentModelId> create,
        IQueryHandler<GetEquipmentModelQuery, EquipmentModelDetails> get,
        CancellationToken cancellationToken
    )
    {
        EquipmentModelId id = await create.HandleAsync(
            new CreateEquipmentModelCommand(
                request.Brand,
                request.Name,
                EquipmentCategoryId.From(request.CategoryId),
                request.TrackingType,
                request.Measures()
            ),
            cancellationToken
        );
        EquipmentModelDetails model = await get.HandleAsync(new GetEquipmentModelQuery(id), cancellationToken);
        return TypedResults.Created($"/api/v1/equipment-models/{id.Value}", model);
    }

    private static async Task<VersionedResult<Ok<EquipmentModelDetails>>> EditAsync(
        Guid modelId,
        EquipmentModelRequest request,
        ICommandHandler<EditEquipmentModelCommand, bool> edit,
        IQueryHandler<GetEquipmentModelQuery, EquipmentModelDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = EquipmentModelId.From(modelId);
        await edit.HandleAsync(
            new EditEquipmentModelCommand(
                id,
                request.Brand,
                request.Name,
                EquipmentCategoryId.From(request.CategoryId),
                request.TrackingType,
                request.Measures()
            ),
            cancellationToken
        );
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<EquipmentModelDetails>>> DeactivateAsync(
        Guid modelId,
        ICommandHandler<DeactivateEquipmentModelCommand, bool> deactivate,
        IQueryHandler<GetEquipmentModelQuery, EquipmentModelDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = EquipmentModelId.From(modelId);
        await deactivate.HandleAsync(new DeactivateEquipmentModelCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<EquipmentModelDetails>>> ActivateAsync(
        Guid modelId,
        ICommandHandler<ActivateEquipmentModelCommand, bool> activate,
        IQueryHandler<GetEquipmentModelQuery, EquipmentModelDetails> get,
        CancellationToken cancellationToken
    )
    {
        var id = EquipmentModelId.From(modelId);
        await activate.HandleAsync(new ActivateEquipmentModelCommand(id), cancellationToken);
        return await CurrentAsync(id, get, cancellationToken);
    }

    // After a change the answer is the model as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<EquipmentModelDetails>>> CurrentAsync(
        EquipmentModelId id,
        IQueryHandler<GetEquipmentModelQuery, EquipmentModelDetails> get,
        CancellationToken cancellationToken
    )
    {
        EquipmentModelDetails model = await get.HandleAsync(new GetEquipmentModelQuery(id), cancellationToken);
        return TypedResults.Ok(model).WithVersion(model.Version);
    }
}
