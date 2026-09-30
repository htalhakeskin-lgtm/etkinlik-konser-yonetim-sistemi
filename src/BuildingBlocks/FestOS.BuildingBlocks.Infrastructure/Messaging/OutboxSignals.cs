using System.Collections.Concurrent;
using System.Threading.Channels;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Wakes a module's dispatcher when a commit wrote events to its outbox (05 §9.2). Signals are
/// coalesced: many commits before the dispatcher wakes are one signal.
/// </summary>
internal sealed class OutboxSignals
{
    private readonly ConcurrentDictionary<string, Channel<bool>> _channels = new(StringComparer.Ordinal);

    /// <summary>Wakes the dispatcher of the module with this schema.</summary>
    public void Notify(string schema) => ChannelFor(schema).Writer.TryWrite(true);

    /// <summary>The signals of the module with this schema.</summary>
    public ChannelReader<bool> ReaderFor(string schema) => ChannelFor(schema).Reader;

    private Channel<bool> ChannelFor(string schema) =>
        _channels.GetOrAdd(
            schema,
            _ =>
                Channel.CreateBounded<bool>(
                    new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite, SingleReader = true }
                )
        );
}
