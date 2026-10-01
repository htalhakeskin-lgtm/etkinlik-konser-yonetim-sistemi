using FestOS.BuildingBlocks.Contracts;
using FestOS.BuildingBlocks.Infrastructure.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>Real-time notifications with SignalR (ADR-0012, building-blocks §11).</summary>
public static class RealtimeExtensions
{
    /// <summary>Registers SignalR with the API's JSON rules and the change publisher the event bus calls.</summary>
    public static IHostApplicationBuilder AddRealtime(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddSignalR().AddJsonProtocol(options => ApiJson.Apply(options.PayloadSerializerOptions));
        builder.Services.AddMetrics();
        builder.Services.TryAddSingleton<RealtimeMetrics>();
        builder.Services.TryAddSingleton<ResourceChangedPublisher>();
        return builder;
    }

    /// <summary>Maps the notification hub at <c>/hubs/notifications</c>; only a signed-in user connects (AT-09).</summary>
    public static IEndpointRouteBuilder MapRealtime(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapHub<NotificationsHub>(NotificationsHub.Path).RequireAuthorization();
        return endpoints;
    }

    /// <summary>
    /// Declares what an integration event changed, so open screens are told to read again (api §13). The
    /// module that owns the event registers it.
    /// </summary>
    public static IServiceCollection AddResourceChange<TIntegrationEvent>(
        this IServiceCollection services,
        Func<TIntegrationEvent, ResourceChange> map
    )
        where TIntegrationEvent : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(map);
        return services.AddSingleton(
            new ResourceChangeMapping(
                typeof(TIntegrationEvent),
                integrationEvent => map((TIntegrationEvent)integrationEvent)
            )
        );
    }

    /// <summary>Registers who may join the groups of one type (building-blocks §11).</summary>
    public static IServiceCollection AddRealtimeGroup<TPolicy>(this IServiceCollection services)
        where TPolicy : class, IRealtimeGroupPolicy => services.AddScoped<IRealtimeGroupPolicy, TPolicy>();
}
