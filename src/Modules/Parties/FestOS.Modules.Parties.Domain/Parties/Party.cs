using System.Text;
using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Rules;
using FestOS.BuildingBlocks.Domain.Text;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>
/// A person or organization the company deals with (05 §5.3, US-PTY-001): an artist, agency, venue
/// operator, supplier or customer, or several of them. Other modules refer to it, so it is never deleted,
/// only deactivated (BR-SYS-001).
/// </summary>
public sealed class Party : AggregateRoot<PartyId>, IDeactivatable
{
    /// <summary>The longest shown name and legal name.</summary>
    public const int NameMaxLength = 200;

    /// <summary>The longest first or last name of a person.</summary>
    public const int PersonNameMaxLength = 100;

    /// <summary>The most contact points a party keeps.</summary>
    public const int MaxContactPoints = 20;

    private readonly List<ContactPoint> _contactPoints = [];
    private List<PartyRole> _roles = [];

    private Party(PartyId id)
        : base(id) { }

    /// <summary>Person or organization; fixed once created (BR-PTY-001).</summary>
    public PartyKind Kind { get; private set; }

    /// <summary>The name lists and pickers show: a stage name, or an organization's short name (PT-02).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>A person's first name; empty for an organization.</summary>
    public string? FirstName { get; private set; }

    /// <summary>A person's last name; empty for an organization.</summary>
    public string? LastName { get; private set; }

    /// <summary>An organization's legal name, if known; empty for a person.</summary>
    public string? LegalName { get; private set; }

    /// <summary>The roles, each once and at least one (BR-PTY-001).</summary>
    public IReadOnlyList<PartyRole> Roles => _roles;

    /// <summary>The contact points in their order.</summary>
    public IReadOnlyList<ContactPoint> ContactPoints => _contactPoints;

    /// <summary>
    /// The search key of the names and contact points, phones also as bare digits (database §13, PT-04).
    /// Rebuilt on every change; the change history has the fields it comes from.
    /// </summary>
    [NotAudited]
    public string Search { get; private set; } = string.Empty;

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <summary>A new, active person.</summary>
    public static Party CreatePerson(
        string name,
        string firstName,
        string lastName,
        IEnumerable<PartyRole> roles,
        IReadOnlyList<ContactPointDetails> contactPoints
    )
    {
        var party = new Party(PartyId.New()) { Kind = PartyKind.Person };
        party.Describe(name, firstName, lastName, legalName: null, roles, contactPoints);
        return party;
    }

    /// <summary>A new, active organization.</summary>
    public static Party CreateOrganization(
        string name,
        string? legalName,
        IEnumerable<PartyRole> roles,
        IReadOnlyList<ContactPointDetails> contactPoints
    )
    {
        var party = new Party(PartyId.New()) { Kind = PartyKind.Organization };
        party.Describe(name, firstName: null, lastName: null, legalName, roles, contactPoints);
        return party;
    }

    /// <summary>
    /// Changes the names, roles and contact points; the kind stays (BR-PTY-001). Names that do not belong
    /// to the kind are dropped.
    /// </summary>
    public void Edit(
        string name,
        string? firstName,
        string? lastName,
        string? legalName,
        IEnumerable<PartyRole> roles,
        IReadOnlyList<ContactPointDetails> contactPoints
    ) => Describe(name, firstName, lastName, legalName, roles, contactPoints);

    /// <summary>Takes the party out of new selections; what already refers to it keeps it (BR-SYS-001).</summary>
    public void Deactivate(Guid deactivatedBy, DateTimeOffset at)
    {
        if (DeactivatedAt is null)
        {
            DeactivatedAt = at;
            DeactivatedBy = deactivatedBy;
        }
    }

    /// <summary>Opens a deactivated party again.</summary>
    public void Activate()
    {
        DeactivatedAt = null;
        DeactivatedBy = null;
    }

    private void Describe(
        string name,
        string? firstName,
        string? lastName,
        string? legalName,
        IEnumerable<PartyRole> roles,
        IReadOnlyList<ContactPointDetails> contactPoints
    )
    {
        List<PartyRole> distinctRoles = [.. roles.Distinct().Order()];
        if (distinctRoles.Count == 0)
        {
            throw new BusinessRuleViolationException(PartiesRuleCodes.PartyRoles, "A party needs at least one role.");
        }

        Name = name.Trim();
        if (Kind == PartyKind.Person)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
            ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            LegalName = null;
        }
        else
        {
            FirstName = null;
            LastName = null;
            LegalName = string.IsNullOrWhiteSpace(legalName) ? null : legalName.Trim();
        }

        _roles = distinctRoles;
        SetContactPoints(contactPoints);
        Search = SearchKey.Of(SearchText());
    }

    // Updates the contact points that come back with their id, adds the rest and removes the ones left out;
    // then each kind gets exactly one primary (BR-PTY-002).
    private void SetContactPoints(IReadOnlyList<ContactPointDetails> contactPoints)
    {
        if (contactPoints.GroupBy(details => details.Kind).Any(kind => kind.Count(details => details.IsPrimary) > 1))
        {
            throw new BusinessRuleViolationException(
                PartiesRuleCodes.PrimaryContactPoint,
                "Each kind of contact point has one primary."
            );
        }

        _contactPoints.RemoveAll(existing => !contactPoints.Any(details => details.Id == existing.Id));
        for (int index = 0; index < contactPoints.Count; index++)
        {
            ContactPointDetails details = contactPoints[index];
            ContactPoint? existing = _contactPoints.Find(contactPoint => contactPoint.Id == details.Id);
            if (existing is null)
            {
                _contactPoints.Add(new ContactPoint(ContactPointId.New(), details, index));
            }
            else
            {
                existing.Change(details, index);
            }
        }

        _contactPoints.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
        foreach (IGrouping<ContactPointKind, ContactPoint> kind in _contactPoints.GroupBy(point => point.Kind))
        {
            if (!kind.Any(contactPoint => contactPoint.IsPrimary))
            {
                kind.First().MakePrimary();
            }
        }
    }

    private string SearchText()
    {
        var text = new StringBuilder(Name);
        foreach (string? part in new[] { FirstName, LastName, LegalName })
        {
            text.Append(' ').Append(part);
        }

        foreach (ContactPoint contactPoint in _contactPoints)
        {
            text.Append(' ').Append(contactPoint.Value);
            if (contactPoint.Kind == ContactPointKind.Phone)
            {
                text.Append(' ').Append(string.Concat(contactPoint.Value.Where(char.IsAsciiDigit)));
            }
        }

        return text.ToString();
    }
}
