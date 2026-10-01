namespace FestOS.BuildingBlocks.Application.Users;

/// <summary>The current user of a host that runs no requests, such as a command of the release script.</summary>
public sealed class SystemCurrentUser : ICurrentUser
{
    /// <inheritdoc />
    public Guid UserId => SystemUser.Id;

    /// <inheritdoc />
    public string DisplayName => SystemUser.Name;
}
