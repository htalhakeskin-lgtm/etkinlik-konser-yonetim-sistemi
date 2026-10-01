using FestOS.BuildingBlocks.Application.Users;

namespace FestOS.Testing;

/// <summary>The current user in tests; a test sets <see cref="UserId"/> to act as someone else.</summary>
public sealed class FakeCurrentUser : ICurrentUser
{
    /// <summary>The user tests act as unless they say otherwise.</summary>
    public static readonly Guid DefaultUserId = new("0192f0a0-0000-7000-8000-000000000042");

    /// <inheritdoc />
    public Guid UserId { get; set; } = DefaultUserId;

    /// <inheritdoc />
    public string DisplayName { get; set; } = "Test Kullanıcısı";
}
