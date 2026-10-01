using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Identity.Domain.Users;

/// <summary>Identifies a <see cref="User"/>.</summary>
public readonly record struct UserId(Guid Value) : IStronglyTypedId<UserId>
{
    /// <inheritdoc />
    public static UserId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static UserId New() => new(Guid.CreateVersion7());
}
