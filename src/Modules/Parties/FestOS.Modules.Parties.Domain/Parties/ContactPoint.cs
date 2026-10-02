using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>A phone number, e-mail or address of a <see cref="Party"/>; part of the party (06 §7).</summary>
public sealed class ContactPoint : Entity<ContactPointId>
{
    /// <summary>The longest value; phones and e-mails are shorter by validation (parties §5).</summary>
    public const int ValueMaxLength = 500;

    /// <summary>The longest label.</summary>
    public const int LabelMaxLength = 100;

    internal ContactPoint(ContactPointId id, ContactPointDetails details, int sortOrder)
        : this(id) => Change(details, sortOrder);

    private ContactPoint(ContactPointId id)
        : base(id) { }

    /// <summary>Phone, e-mail or address.</summary>
    public ContactPointKind Kind { get; private set; }

    /// <summary>The number, address or e-mail; e-mails are kept in lower case.</summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>An optional label.</summary>
    public string? Label { get; private set; }

    /// <summary>Whether it is the primary of its kind (BR-PTY-002).</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>Its place in the party's list.</summary>
    public int SortOrder { get; private set; }

    internal void Change(ContactPointDetails details, int sortOrder)
    {
        Kind = details.Kind;
        Value = details.Kind == ContactPointKind.Email ? details.Value.Trim().ToLowerInvariant() : details.Value.Trim();
        Label = string.IsNullOrWhiteSpace(details.Label) ? null : details.Label.Trim();
        IsPrimary = details.IsPrimary;
        SortOrder = sortOrder;
    }

    internal void MakePrimary() => IsPrimary = true;
}
