using System.Reflection;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Contracts;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.BuildingBlocks.Infrastructure.Realtime;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Delivers events inside the process. Each listener gets its own scope, since listeners of different
/// modules run in parallel in their own transactions, acting as the system user (database §9).
/// </summary>
internal sealed class InProcessEventBus(IServiceScopeFactory scopes, ResourceChangedPublisher? resourceChanges = null)
    : IEventBus
{
    private static readonly MethodInfo DeliverMethod = typeof(InProcessEventBus).GetMethod(
        nameof(DeliverAsync),
        BindingFlags.NonPublic | BindingFlags.Static
    )!;

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        // Open screens hear of the change first; it is committed already (building-blocks §11).
        if (resourceChanges is not null)
        {
            await resourceChanges.PublishAsync(integrationEvent, cancellationToken);
        }

        Type eventType = integrationEvent.GetType();
        Type handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

        int listenerCount;
        await using (AsyncServiceScope scope = scopes.CreateAsyncScope())
        {
            listenerCount = scope.ServiceProvider.GetServices(handlerType).Count();
        }

        MethodInfo deliver = DeliverMethod.MakeGenericMethod(eventType);
        var all = Task.WhenAll(
            Enumerable
                .Range(0, listenerCount)
                .Select(index => (Task)deliver.Invoke(null, [scopes, index, integrationEvent, cancellationToken])!)
        );

        try
        {
            await all;
        }
        catch when (all.Exception is { } failures)
        {
            throw failures.Flatten();
        }
    }

    private static async Task DeliverAsync<TIntegrationEvent>(
        IServiceScopeFactory scopes,
        int index,
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken
    )
        where TIntegrationEvent : IIntegrationEvent
    {
        await using AsyncServiceScope scope = scopes.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ActingUser>().ActAsSystem();
        IIntegrationEventHandler<TIntegrationEvent> listener = scope
            .ServiceProvider.GetServices<IIntegrationEventHandler<TIntegrationEvent>>()
            .ElementAt(index);
        await listener.HandleAsync(integrationEvent, cancellationToken);
    }
}
