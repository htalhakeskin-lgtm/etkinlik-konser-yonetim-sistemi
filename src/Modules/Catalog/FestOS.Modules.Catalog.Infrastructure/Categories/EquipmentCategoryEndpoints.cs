using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Catalog.Application.Categories;
using FestOS.Modules.Catalog.Contracts;
using FestOS.Modules.Catalog.Domain.Categories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Catalog.Infrastructure.Categories;

/// <summary>The category tree (catalog §6).</summary>
internal static class EquipmentCategoryEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/equipment-categories", ListAsync)
            .RequirePermission(CatalogPermissions.ViewCategories)
            .WithName("ListEquipmentCategories")
            .WithSummary("Lists the category tree as a flat list with each category's path.");
        endpoints
            .MapPost("/equipment-categories", CreateAsync)
            .RequirePermission(CatalogPermissions.CreateCategories)
            .WithName("CreateEquipmentCategory")
            .WithSummary("Adds a category under a parent or at the top.");
        endpoints
            .MapPut("/equipment-categories/{categoryId:guid}", EditAsync)
            .RequirePermission(CatalogPermissions.EditCategories)
            .RequiresVersion()
            .WithName("EditEquipmentCategory")
            .WithSummary("Renames a category or moves it under another parent.");
        endpoints
            .MapPost("/equipment-categories/{categoryId:guid}/deactivate", DeactivateAsync)
            .RequirePermission(CatalogPermissions.DeactivateCategories)
            .RequiresVersion()
            .WithName("DeactivateEquipmentCategory")
            .WithSummary("Deactivates a category with nothing active under it.");
        endpoints
            .MapPost("/equipment-categories/{categoryId:guid}/activate", ActivateAsync)
            .RequirePermission(CatalogPermissions.DeactivateCategories)
            .RequiresVersion()
            .WithName("ActivateEquipmentCategory")
            .WithSummary("Activates a deactivated category under an active parent.");
    }

    private static async Task<Ok<IReadOnlyList<EquipmentCategoryItem>>> ListAsync(
        [AsParameters] ListEquipmentCategoriesRequest request,
        IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>> list,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await list.HandleAsync(
                new ListEquipmentCategoriesQuery(request.Status?.Value ?? CategoryStatusFilter.Active),
                cancellationToken
            )
        );

    private static async Task<Created<EquipmentCategoryItem>> CreateAsync(
        EquipmentCategoryRequest request,
        ICommandHandler<CreateEquipmentCategoryCommand, EquipmentCategoryId> create,
        IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>> list,
        CancellationToken cancellationToken
    )
    {
        EquipmentCategoryId id = await create.HandleAsync(
            new CreateEquipmentCategoryCommand(request.Name, ParentOf(request)),
            cancellationToken
        );
        EquipmentCategoryItem category = await CategoryAsync(id, list, cancellationToken);
        return TypedResults.Created($"/api/v1/equipment-categories/{id.Value}", category);
    }

    private static async Task<VersionedResult<Ok<EquipmentCategoryItem>>> EditAsync(
        Guid categoryId,
        EquipmentCategoryRequest request,
        ICommandHandler<EditEquipmentCategoryCommand, bool> edit,
        IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>> list,
        CancellationToken cancellationToken
    )
    {
        var id = EquipmentCategoryId.From(categoryId);
        await edit.HandleAsync(
            new EditEquipmentCategoryCommand(id, request.Name, ParentOf(request)),
            cancellationToken
        );
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<EquipmentCategoryItem>>> DeactivateAsync(
        Guid categoryId,
        ICommandHandler<DeactivateEquipmentCategoryCommand, bool> deactivate,
        IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>> list,
        CancellationToken cancellationToken
    )
    {
        var id = EquipmentCategoryId.From(categoryId);
        await deactivate.HandleAsync(new DeactivateEquipmentCategoryCommand(id), cancellationToken);
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<EquipmentCategoryItem>>> ActivateAsync(
        Guid categoryId,
        ICommandHandler<ActivateEquipmentCategoryCommand, bool> activate,
        IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>> list,
        CancellationToken cancellationToken
    )
    {
        var id = EquipmentCategoryId.From(categoryId);
        await activate.HandleAsync(new ActivateEquipmentCategoryCommand(id), cancellationToken);
        return await CurrentAsync(id, list, cancellationToken);
    }

    private static EquipmentCategoryId? ParentOf(EquipmentCategoryRequest request) =>
        request.ParentId is { } parent ? EquipmentCategoryId.From(parent) : null;

    // After a change the answer is the category as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<EquipmentCategoryItem>>> CurrentAsync(
        EquipmentCategoryId id,
        IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>> list,
        CancellationToken cancellationToken
    )
    {
        EquipmentCategoryItem category = await CategoryAsync(id, list, cancellationToken);
        return TypedResults.Ok(category).WithVersion(category.Version);
    }

    private static async Task<EquipmentCategoryItem> CategoryAsync(
        EquipmentCategoryId id,
        IQueryHandler<ListEquipmentCategoriesQuery, IReadOnlyList<EquipmentCategoryItem>> list,
        CancellationToken cancellationToken
    ) =>
        (
            await list.HandleAsync(new ListEquipmentCategoriesQuery(CategoryStatusFilter.All), cancellationToken)
        ).SingleOrDefault(category => category.Id == id) ?? throw new NotFoundException("EquipmentCategory", id.Value);
}
