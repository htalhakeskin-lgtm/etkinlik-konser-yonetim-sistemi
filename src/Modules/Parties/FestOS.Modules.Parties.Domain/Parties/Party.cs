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
    private readonly List<OrganizationContact> _contactPersons = [];
    private readonly List<ArtistRepresentation> _representations = [];
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

    /// <summary>The people who speak for an organization, in their order (BR-PTY-003).</summary>
    public IReadOnlyList<OrganizationContact> ContactPersons => _contactPersons;

    /// <summary>The agencies that represent an artist (BR-PTY-004).</summary>
    public IReadOnlyList<ArtistRepresentation> Representations => _representations;

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

    /// <summary>
    /// Ties a person to this organization (BR-PTY-003): only an organization takes contact persons, only an
    /// active person can be one, and each once.
    /// </summary>
    public OrganizationContact AddContactPerson(Party person, string? title)
    {
        if (Kind != PartyKind.Organization || person.Kind != PartyKind.Person || person.DeactivatedAt is not null)
        {
            throw new BusinessRuleViolationException(
                PartiesRuleCodes.ContactPerson,
                "A contact person is an active person tied to an organization."
            );
        }

        if (_contactPersons.Exists(contact => contact.PersonId == person.Id))
        {
            throw new BusinessRuleViolationException(
                PartiesRuleCodes.ContactPerson,
                "The person is already a contact of this organization."
            );
        }

        var contact = new OrganizationContact(
            OrganizationContactId.New(),
            person.Id,
            title,
            _contactPersons.Count == 0 ? 0 : _contactPersons.Max(existing => existing.SortOrder) + 1
        );
        _contactPersons.Add(contact);
        return contact;
    }

    /// <summary>Changes a contact person's job title.</summary>
    public void RetitleContactPerson(OrganizationContactId id, string? title) => ContactPerson(id).Retitle(title);

    /// <summary>Unties a contact person; the person stays a party.</summary>
    public void RemoveContactPerson(OrganizationContactId id) => _contactPersons.Remove(ContactPerson(id));

    /// <summary>
    /// Records that an agency represents this artist (BR-PTY-004): both active, this one an artist, the other an
    /// agency, and each agency once.
    /// </summary>
    public ArtistRepresentation AddRepresentation(Party agency, string? description)
    {
        if (
            DeactivatedAt is not null
            || !_roles.Contains(PartyRole.Artist)
            || agency.DeactivatedAt is not null
            || !agency.Roles.Contains(PartyRole.Agency)
        )
        {
            throw new BusinessRuleViolationException(
                PartiesRuleCodes.RoleRequiredSelection,
                "A representation ties an active artist to an active agency.",
                parameters: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["field"] = "agencyId",
                    ["role"] = "agency",
                }
            );
        }

        if (_representations.Exists(representation => representation.AgencyId == agency.Id))
        {
            throw new BusinessRuleViolationException(
                PartiesRuleCodes.RoleRequiredSelection,
                "The agency already represents this artist."
            );
        }

        var representation = new ArtistRepresentation(ArtistRepresentationId.New(), agency.Id, description);
        _representations.Add(representation);
        return representation;
    }

    /// <summary>Changes what a representation covers.</summary>
    public void DescribeRepresentation(ArtistRepresentationId id, string? description) =>
        Representation(id).Describe(description);

    /// <summary>Ends a representation.</summary>
    public void RemoveRepresentation(ArtistRepresentationId id) => _representations.Remove(Representation(id));

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

        // An artist's representations need the artist role (BR-PTY-004); the agency side is the command's check.
        if (_representations.Count > 0 && !distinctRoles.Contains(PartyRole.Artist))
        {
            throw new BusinessRuleViolationException(
                PartiesRuleCodes.RoleRequiredSelection,
                "An artist with representations keeps the artist role.",
                parameters: new Dictionary<string, object?>(StringComparer.Ordinal) { ["role"] = "artist" }
            );
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

    private OrganizationContact ContactPerson(OrganizationContactId id) =>
        _contactPersons.Find(contact => contact.Id == id) ?? throw new KeyNotFoundException("No such contact person.");

    private ArtistRepresentation Representation(ArtistRepresentationId id) =>
        _representations.Find(representation => representation.Id == id)
        ?? throw new KeyNotFoundException("No such representation.");

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
