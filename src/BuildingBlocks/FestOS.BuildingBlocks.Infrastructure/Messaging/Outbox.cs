using System.Diagnostics;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FestOS.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// The events added during the current unit of work; save step 5 moves them into the saving module's
/// <c>outbox_messages</c>, so they commit or roll back with the change.
/// </summary>
internal sealed class Outbox : IOutbox
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly List<IIntegrationEvent> _pending = [];

    /// <summary>Whether the last save of this unit of work wrote events, so the commit wakes the dispatcher.</summary>
    public bool WroteMessages { get; private set; }

    public void Add(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        _pending.Add(integrationEvent);
    }

    /// <summary>Forgets what a failed attempt added; the retried handler adds its events again.</summary>
    public void Clear()
    {
        _pending.Clear();
        WroteMessages = false;
    }

    public void MoveTo(DbContext context)
    {
        string? traceParent = Activity.Current?.Id;
        foreach (IIntegrationEvent integrationEvent in _pending)
        {
            Type type = integrationEvent.GetType();
            context
                .Set<OutboxMessage>()
                .Add(
                    new OutboxMessage
                    {
                        Id = integrationEvent.MessageId,
                        Type = TypeName(type),
                        OrderingKey = integrationEvent.OrderingKey,
                        Payload = JsonSerializer.Serialize(integrationEvent, type, JsonOptions),
                        OccurredAt = integrationEvent.OccurredAt,
                        TraceParent = traceParent,
                    }
                );
        }

        WroteMessages |= _pending.Count > 0;
        _pending.Clear();
    }

    // Without version and key, so a new build still reads older messages.
    public static string TypeName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";
}
