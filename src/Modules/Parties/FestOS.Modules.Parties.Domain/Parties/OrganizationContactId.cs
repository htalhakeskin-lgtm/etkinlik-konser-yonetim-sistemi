using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>Identifies an <see cref="OrganizationContact"/>.</summary>
public readonly record struct OrganizationContactId(Guid Value) : IStronglyTypedId<OrganizationContactId>
{
    /// <inheritdoc />
    public static OrganizationContactId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static OrganizationContactId New() => new(Guid.CreateVersion7());
}
