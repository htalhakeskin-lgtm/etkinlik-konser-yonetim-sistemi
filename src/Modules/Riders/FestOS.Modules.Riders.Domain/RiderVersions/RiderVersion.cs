using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.Modules.Riders.Domain.Riders;

namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>
/// A rider as saved once, with its lines (US-RDR-001); it never changes, a change is the next version
/// (06 §8). Being its own history, it writes no change history rows.
/// </summary>
[NotAudited]
public sealed class RiderVersion : ImmutableRecord<RiderVersionId>
{
    /// <summary>The most lines a version has.</summary>
    public const int MaxLines = 500;

    /// <summary>The longest note.</summary>
    public const int NoteMaxLength = 500;

    /// <summary>The longest name of whoever saved it.</summary>
    public const int CreatedByNameMaxLength = 200;

    private readonly List<RiderLine> _lines = [];

    internal RiderVersion(
        RiderId riderId,
        int number,
        IReadOnlyList<RiderLineDetails> lines,
        string? note,
        string createdByName
    )
        : this(RiderVersionId.New())
    {
        RiderId = riderId;
        Number = number;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        CreatedByName = createdByName;
        _lines.AddRange(lines.Select((line, index) => new RiderLine(RiderLineId.New(), line, index)));
    }

    private RiderVersion(RiderVersionId id)
        : base(id) { }

    /// <summary>The rider.</summary>
    public RiderId RiderId { get; private set; }

    /// <summary>Its number in the rider, from 1.</summary>
    public int Number { get; private set; }

    /// <summary>Why this version, such as "2027 tour rider" (riders RD-05).</summary>
    public string? Note { get; private set; }

    /// <summary>The name of whoever saved it, as it was then; the list needs no call to Identity.</summary>
    public string CreatedByName { get; private set; } = string.Empty;

    /// <summary>The lines, in order.</summary>
    public IReadOnlyList<RiderLine> Lines => _lines;
}
