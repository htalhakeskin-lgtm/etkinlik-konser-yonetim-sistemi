using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

namespace FestOS.BuildingBlocks.Infrastructure.Realtime;

/// <summary>
/// The notification hub at <c>/hubs/notifications</c> (ADR-0012, api §13). Clients join the groups of the
/// screens they show; a group is joined only when a module's policy allows it (building-blocks §11).
/// </summary>
public sealed partial class NotificationsHub(IEnumerable<IRealtimeGroupPolicy> policies, RealtimeMetrics metrics) : Hub
{
    /// <summary>The hub's address.</summary>
    public const string Path = "/hubs/notifications";

    /// <summary>The client method every notification calls.</summary>
    public const string ResourceChangedMethod = "resourceChanged";

    /// <summary>Joins a group, e.g. <c>warehouses:{id}</c> or the list group <c>equipment-units</c>.</summary>
    [HubMethodName("JoinGroup")]
    public async Task JoinGroupAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Match group = GroupName().Match(name);
        if (!group.Success)
        {
            throw new HubException($"'{name}' is not a group name (naming §8.3).");
        }

        string type = group.Groups["type"].Value;
        string? id = group.Groups["id"].Success ? group.Groups["id"].Value : null;
        IRealtimeGroupPolicy policy =
            policies.FirstOrDefault(candidate => string.Equals(candidate.GroupType, type, StringComparison.Ordinal))
            ?? throw new HubException($"No module lets clients join '{type}' groups.");

        if (!await policy.CanJoinAsync(Context.User ?? new(), id, Context.ConnectionAborted))
        {
            throw new HubException($"The user may not join '{name}'.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, name, Context.ConnectionAborted);
    }

    /// <summary>Leaves a group, e.g. when the screen showing it closes.</summary>
    [HubMethodName("LeaveGroup")]
    public Task LeaveGroupAsync(string name) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, name, Context.ConnectionAborted);

    /// <inheritdoc />
    public override Task OnConnectedAsync()
    {
        metrics.Connected();
        return base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        metrics.Disconnected();
        return base.OnDisconnectedAsync(exception);
    }

    [GeneratedRegex("^(?<type>[a-z][a-z-]{0,62})(:(?<id>[A-Za-z0-9-]{1,64}))?$", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex GroupName();
}
