using System.Security.Claims;
using FestOS.BuildingBlocks.Infrastructure.Realtime;

namespace FestOS.Modules.Sample.Infrastructure;

/// <summary>Lets every client join the sample item groups; a real module checks the user's access here.</summary>
internal sealed class SampleItemGroupPolicy : IRealtimeGroupPolicy
{
    public string GroupType => "sample-items";

    public ValueTask<bool> CanJoinAsync(ClaimsPrincipal user, string? id, CancellationToken cancellationToken) =>
        ValueTask.FromResult(true);
}
