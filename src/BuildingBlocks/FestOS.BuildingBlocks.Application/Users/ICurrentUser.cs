namespace FestOS.BuildingBlocks.Application.Users;

/// <summary>The user on whose behalf the current operation runs.</summary>
public interface ICurrentUser
{
    /// <summary>
    /// The signed-in user's identifier, or <see cref="SystemUser.Id"/> for work that no user started,
    /// such as scheduled jobs and event handlers (database §9).
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// The user's name as the change history shows it, kept as it is at that moment (audit AU-01);
    /// <see cref="SystemUser.Name"/> for work that no user started.
    /// </summary>
    string DisplayName { get; }
}
