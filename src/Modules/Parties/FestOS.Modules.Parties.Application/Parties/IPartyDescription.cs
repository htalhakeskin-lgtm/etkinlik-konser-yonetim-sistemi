using FestOS.Modules.Parties.Domain.Parties;

namespace FestOS.Modules.Parties.Application.Parties;

/// <summary>What creating and editing a party both send, so one set of checks covers both (parties §8).</summary>
public interface IPartyDescription
{
    /// <summary>The shown name.</summary>
    string Name { get; }

    /// <summary>A person's first name.</summary>
    string? FirstName { get; }

    /// <summary>A person's last name.</summary>
    string? LastName { get; }

    /// <summary>An organization's legal name.</summary>
    string? LegalName { get; }

    /// <summary>The roles.</summary>
    IReadOnlyList<PartyRole> Roles { get; }

    /// <summary>The contact points in their order.</summary>
    IReadOnlyList<ContactPointDetails> ContactPoints { get; }
}
