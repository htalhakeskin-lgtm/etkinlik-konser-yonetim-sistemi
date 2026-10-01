using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Inventory.Application.Warehouses;
using FestOS.Modules.Inventory.Contracts;
using FestOS.Modules.Inventory.Domain.Warehouses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Inventory.Infrastructure.Warehouses;

/// <summary>The warehouses (inventory §5).</summary>
internal static class WarehouseEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/warehouses", ListWarehousesAsync)
            .RequirePermission(InventoryPermissions.ViewWarehouses)
            .WithName("ListWarehouses")
            .WithSummary("Lists the warehouses, searched and filtered, one page at a time.");
        endpoints
            .MapGet("/warehouses/{warehouseId:guid}", GetWarehouseAsync)
            .RequirePermission(InventoryPermissions.ViewWarehouses)
            .WithName("GetWarehouse")
            .WithSummary("Gets a warehouse.");
        endpoints
            .MapPost("/warehouses", CreateWarehouseAsync)
            .RequirePermission(InventoryPermissions.CreateWarehouses)
            .WithName("CreateWarehouse")
            .WithSummary("Creates a warehouse.");
        endpoints
            .MapPut("/warehouses/{warehouseId:guid}", EditWarehouseAsync)
            .RequirePermission(InventoryPermissions.EditWarehouses)
            .RequiresVersion()
            .WithName("EditWarehouse")
            .WithSummary("Changes a warehouse's name, city and address.");
        endpoints
            .MapPost("/warehouses/{warehouseId:guid}/deactivate", DeactivateWarehouseAsync)
            .RequirePermission(InventoryPermissions.DeactivateWarehouses)
            .RequiresVersion()
            .WithName("DeactivateWarehouse")
            .WithSummary("Deactivates a warehouse; the last active one stays.");
        endpoints
            .MapPost("/warehouses/{warehouseId:guid}/activate", ActivateWarehouseAsync)
            .RequirePermission(InventoryPermissions.DeactivateWarehouses)
            .RequiresVersion()
            .WithName("ActivateWarehouse")
            .WithSummary("Activates a deactivated warehouse again.");
    }

    private static async Task<Ok<PagedResult<WarehouseListItem>>> ListWarehousesAsync(
        [AsParameters] ListWarehousesRequest request,
        IQueryHandler<ListWarehousesQuery, PagedResult<WarehouseListItem>> listWarehouses,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await listWarehouses.HandleAsync(
                new ListWarehousesQuery(
                    request.Q,
                    request.Status?.Value ?? WarehouseStatusFilter.Active,
                    request.Sort,
                    new PageRequest(request.Page ?? 1, request.PageSize ?? PageRequest.DefaultPageSize)
                ),
                cancellationToken
            )
        );

    private static async Task<VersionedResult<Ok<WarehouseDetails>>> GetWarehouseAsync(
        Guid warehouseId,
        IQueryHandler<GetWarehouseQuery, WarehouseDetails> getWarehouse,
        CancellationToken cancellationToken
    ) => await CurrentAsync(WarehouseId.From(warehouseId), getWarehouse, cancellationToken);

    private static async Task<Created<WarehouseDetails>> CreateWarehouseAsync(
        WarehouseRequest request,
        ICommandHandler<CreateWarehouseCommand, WarehouseId> createWarehouse,
        IQueryHandler<GetWarehouseQuery, WarehouseDetails> getWarehouse,
        CancellationToken cancellationToken
    )
    {
        WarehouseId id = await createWarehouse.HandleAsync(
            new CreateWarehouseCommand(request.Name, request.City, request.Address),
            cancellationToken
        );
        WarehouseDetails warehouse = await getWarehouse.HandleAsync(new GetWarehouseQuery(id), cancellationToken);
        return TypedResults.Created($"/api/v1/warehouses/{id.Value}", warehouse);
    }

    private static async Task<VersionedResult<Ok<WarehouseDetails>>> EditWarehouseAsync(
        Guid warehouseId,
        WarehouseRequest request,
        ICommandHandler<EditWarehouseCommand, bool> editWarehouse,
        IQueryHandler<GetWarehouseQuery, WarehouseDetails> getWarehouse,
        CancellationToken cancellationToken
    )
    {
        var id = WarehouseId.From(warehouseId);
        await editWarehouse.HandleAsync(
            new EditWarehouseCommand(id, request.Name, request.City, request.Address),
            cancellationToken
        );
        return await CurrentAsync(id, getWarehouse, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<WarehouseDetails>>> DeactivateWarehouseAsync(
        Guid warehouseId,
        ICommandHandler<DeactivateWarehouseCommand, bool> deactivateWarehouse,
        IQueryHandler<GetWarehouseQuery, WarehouseDetails> getWarehouse,
        CancellationToken cancellationToken
    )
    {
        var id = WarehouseId.From(warehouseId);
        await deactivateWarehouse.HandleAsync(new DeactivateWarehouseCommand(id), cancellationToken);
        return await CurrentAsync(id, getWarehouse, cancellationToken);
    }

    private static async Task<VersionedResult<Ok<WarehouseDetails>>> ActivateWarehouseAsync(
        Guid warehouseId,
        ICommandHandler<ActivateWarehouseCommand, bool> activateWarehouse,
        IQueryHandler<GetWarehouseQuery, WarehouseDetails> getWarehouse,
        CancellationToken cancellationToken
    )
    {
        var id = WarehouseId.From(warehouseId);
        await activateWarehouse.HandleAsync(new ActivateWarehouseCommand(id), cancellationToken);
        return await CurrentAsync(id, getWarehouse, cancellationToken);
    }

    // After a change the answer is the warehouse as saved, with the new version (api §9).
    private static async Task<VersionedResult<Ok<WarehouseDetails>>> CurrentAsync(
        WarehouseId id,
        IQueryHandler<GetWarehouseQuery, WarehouseDetails> getWarehouse,
        CancellationToken cancellationToken
    )
    {
        WarehouseDetails warehouse = await getWarehouse.HandleAsync(new GetWarehouseQuery(id), cancellationToken);
        return TypedResults.Ok(warehouse).WithVersion(warehouse.Version);
    }
}
