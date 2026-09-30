using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// The event delivery metrics of observability §5. Tags are bounded sets, the module and the event
/// type; never a record identifier.
/// </summary>
public sealed class MessagingMetrics
{
    /// <summary>The meter, exported through <c>FestOS.*</c>.</summary>
    public const string MeterName = "FestOS.BuildingBlocks";

    private readonly Histogram<double> _latency;
    private readonly Counter<long> _deadLetters;
    private readonly ConcurrentDictionary<string, long> _pending = new(StringComparer.Ordinal);

    /// <summary>Creates the instruments on a meter from the factory.</summary>
    public MessagingMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);
        Meter meter = meterFactory.Create(MeterName);
        _latency = meter.CreateHistogram<double>(
            "festos.messaging.event.latency",
            unit: "s",
            description: "Time from an event's occurrence to its delivery to every listener."
        );
        _deadLetters = meter.CreateCounter<long>(
            "festos.messaging.dead_letters",
            unit: "{event}",
            description: "Events marked failed after all delivery attempts."
        );
        meter.CreateObservableGauge(
            "festos.messaging.outbox.pending",
            () =>
                _pending.Select(pair => new Measurement<long>(
                    pair.Value,
                    new KeyValuePair<string, object?>("festos.module", pair.Key)
                )),
            unit: "{event}",
            description: "Events waiting to be delivered."
        );
    }

    /// <summary>Records how long a delivered event took.</summary>
    public void RecordDelivered(string module, string eventName, TimeSpan latency) =>
        _latency.Record(latency.TotalSeconds, new("festos.module", module), new("festos.event.type", eventName));

    /// <summary>Counts an event marked failed.</summary>
    public void RecordFailed(string module, string eventName) =>
        _deadLetters.Add(1, new("festos.module", module), new("festos.event.type", eventName));

    /// <summary>Updates the module's number of waiting events.</summary>
    public void SetPending(string module, long count) => _pending[module] = count;
}
