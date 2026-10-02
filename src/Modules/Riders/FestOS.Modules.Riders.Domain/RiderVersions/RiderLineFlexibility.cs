namespace FestOS.Modules.Riders.Domain.RiderVersions;

/// <summary>Whether a model line takes only its model or also its equivalents (US-RDR-001).</summary>
public enum RiderLineFlexibility
{
    /// <summary>Only this model will do.</summary>
    Required,

    /// <summary>The model or one of its equivalents, in their order.</summary>
    Flexible,
}
