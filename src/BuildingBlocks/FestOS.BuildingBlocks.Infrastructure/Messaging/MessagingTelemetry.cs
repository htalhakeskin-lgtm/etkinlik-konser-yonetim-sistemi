using System.Diagnostics;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Traces of the event delivery (observability §4).</summary>
internal static class MessagingTelemetry
{
    /// <summary>The same source as the command and query spans, exported through <c>FestOS.*</c>.</summary>
    public static readonly ActivitySource Source = new("FestOS.BuildingBlocks");

    /// <summary>
    /// Starts the span of one delivery as a child of the request that raised the event, so both
    /// modules' work appears in one trace.
    /// </summary>
    public static Activity? StartDelivery(OutboxMessage message, string moduleName, string eventName)
    {
        ActivityContext parent = ActivityContext.TryParse(
            message.TraceParent,
            traceState: null,
            out ActivityContext parsed
        )
            ? parsed
            : default;
        Activity? activity = Source.StartActivity($"Deliver {eventName}", ActivityKind.Consumer, parent);
        activity?.SetTag("festos.module", moduleName);
        activity?.SetTag("festos.message.id", message.Id);
        return activity;
    }
}
