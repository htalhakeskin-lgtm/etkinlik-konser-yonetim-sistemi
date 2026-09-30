using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.Modules.Sample.Application;
using FestOS.Modules.Sample.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Sample.Api;

/// <summary>The sample module's endpoints, for the platform's HTTP tests.</summary>
internal static class SampleEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/sample-items", CreateAsync);
        endpoints.MapPut("/sample-items/{id:guid}/use", UseAsync).RequiresVersion();
    }

    private static async Task<Created<SampleItemCreatedResponse>> CreateAsync(
        CreateSampleItemRequest request,
        ICommandHandler<CreateSampleItemCommand, SampleItemId> create,
        CancellationToken cancellationToken
    )
    {
        SampleItemId id = await create.HandleAsync(new(request.Name, request.UnitPrice), cancellationToken);
        return TypedResults.Created($"/api/v1/sample-items/{id.Value}", new SampleItemCreatedResponse(id));
    }

    private static async Task<NoContent> UseAsync(
        Guid id,
        ICommandHandler<UseSampleItemCommand, bool> use,
        CancellationToken cancellationToken
    )
    {
        await use.HandleAsync(new(new SampleItemId(id)), cancellationToken);
        return TypedResults.NoContent();
    }
}
