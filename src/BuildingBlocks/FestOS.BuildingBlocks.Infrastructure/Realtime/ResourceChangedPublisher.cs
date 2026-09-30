using FestOS.BuildingBlocks.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>
/// Tells the open screens what an integration event changed (building-blocks §11): the event bus calls it
/// for every delivered event, before the module listeners, and the modules' mappings decide the resource
/// and the groups. A notification is only a hint to read again, so a failed one is logged, never retried.
/// </summary>
internal sealed partial class ResourceChangedPublisher(
    IHubContext<NotificationsHub> hub,
    IEnumerable<ResourceChangeMapping> mappings,
    ILogger<ResourceChangedPublisher> logger
)
{
    private readonly Dictionary<Type, ResourceChangeMapping> _mappings = mappings.ToDictionary(mapping =>
        mapping.EventType
    );

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        if (!_mappings.TryGetValue(integrationEvent.GetType(), out ResourceChangeMapping? mapping))
        {
            return;
        }

        ResourceChange change = mapping.Map(integrationEvent);
        try
        {
            await hub
                .Clients.Groups(change.Groups)
                .SendAsync(
                    NotificationsHub.ResourceChangedMethod,
                    new ResourceChangedMessage(change.Resource, change.Id, change.Version),
                    cancellationToken
                );
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Clients read everything again when they reconnect (api §13).
            LogNotificationFailed(logger, exception, change.Resource, change.Id);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Notifying the change of {Resource} {ResourceId} failed")]
    private static partial void LogNotificationFailed(
        ILogger logger,
        Exception exception,
        string resource,
        Guid resourceId
    );
}
