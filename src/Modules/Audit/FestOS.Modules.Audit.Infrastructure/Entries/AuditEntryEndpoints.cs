using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Paging;
using FestOS.BuildingBlocks.Infrastructure.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FestOS.Modules.Audit.Infrastructure.Entries;

/// <summary>The change history (audit §3); read-only, as the table is (DT-02).</summary>
internal static class AuditEntryEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet("/audit-entries", ListAuditEntriesAsync)
            .RequirePermission(AuditPermissions.ViewEntries)
            .WithName("ListAuditEntries")
            .WithSummary("Lists the change history from newest to oldest, filtered, one slice at a time.");

    private static async Task<Ok<CursorResult<AuditEntryItem>>> ListAuditEntriesAsync(
        [AsParameters] ListAuditEntriesRequest request,
        IQueryHandler<ListAuditEntriesQuery, CursorResult<AuditEntryItem>> listEntries,
        CancellationToken cancellationToken
    ) =>
        TypedResults.Ok(
            await listEntries.HandleAsync(
                new ListAuditEntriesQuery(
                    request.ActorId,
                    request.From,
                    request.To,
                    request.Module,
                    request.EntityType,
                    request.EntityId,
                    new CursorRequest(request.After, request.Limit ?? CursorRequest.DefaultLimit)
                ),
                cancellationToken
            )
        );
}
