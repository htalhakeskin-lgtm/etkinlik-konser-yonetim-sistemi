using FestOS.BuildingBlocks.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The user recorded as making the changes of this scope: the current user, or the system user for work
/// no user started, such as event listeners and scheduled jobs (database §9).
/// </summary>
internal sealed class ActingUser(IServiceProvider services)
{
    private bool _actsAsSystem;

    // Background scopes need no ICurrentUser, so it is resolved only when used.
    public Guid UserId => _actsAsSystem ? SystemUser.Id : services.GetRequiredService<ICurrentUser>().UserId;

    public string DisplayName =>
        _actsAsSystem ? SystemUser.Name : services.GetRequiredService<ICurrentUser>().DisplayName;

    public void ActAsSystem() => _actsAsSystem = true;
}
