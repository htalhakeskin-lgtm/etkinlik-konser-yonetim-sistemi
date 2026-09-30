using System.Diagnostics.Metrics;
using FestOS.BuildingBlocks.Infrastructure.Messaging;

namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>The open notification connections (observability §5).</summary>
public sealed class RealtimeMetrics
{
    private readonly UpDownCounter<long> _connections;

    /// <summary>Creates the instruments on the platform's meter.</summary>
    public RealtimeMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);
        _connections = meterFactory
            .Create(MessagingMetrics.MeterName)
            .CreateUpDownCounter<long>(
                "festos.realtime.connections",
                unit: "{connection}",
                description: "Open SignalR connections."
            );
    }

    /// <summary>Counts a new connection.</summary>
    public void Connected() => _connections.Add(1);

    /// <summary>Counts a closed connection.</summary>
    public void Disconnected() => _connections.Add(-1);
}
