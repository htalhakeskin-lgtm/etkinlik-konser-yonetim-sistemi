namespace FestOS.Modules.Catalog.Domain.Models;

/// <summary>How a model's stock is counted (01 §3.6); fixed once stock exists (BR-EQP-001).</summary>
public enum TrackingType
{
    /// <summary>Each unit has a serial number and its own label.</summary>
    Serialized,

    /// <summary>Counted as a quantity, e.g. cables.</summary>
    Bulk,
}
