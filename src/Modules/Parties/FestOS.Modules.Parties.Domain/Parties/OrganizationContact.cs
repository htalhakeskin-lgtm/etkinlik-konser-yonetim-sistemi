using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>
/// A person who speaks for an organization (BR-PTY-003); part of the organization (06 §7). Removing it unties
/// the two, the person stays.
/// </summary>
public sealed class OrganizationContact : Entity<OrganizationContactId>
{
    /// <summary>The longest job title.</summary>
    public const int TitleMaxLength = 100;

    internal OrganizationContact(OrganizationContactId id, PartyId personId, string? title, int sortOrder)
        : this(id)
    {
        PersonId = personId;
        SortOrder = sortOrder;
        Retitle(title);
    }

    private OrganizationContact(OrganizationContactId id)
        : base(id) { }

    /// <summary>The person, a party of the person kind.</summary>
    public PartyId PersonId { get; private set; }

    /// <summary>The person's job at the organization, if known.</summary>
    public string? Title { get; private set; }

    /// <summary>Its place in the organization's list.</summary>
    public int SortOrder { get; private set; }

    internal void Retitle(string? title) => Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
}
