namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>Which equipment lines the venue page shows by their validity.</summary>
public enum EquipmentStatusFilter
{
    /// <summary>Lines not ended yet, upcoming ones included; the default.</summary>
    Current,

    /// <summary>Lines whose validity has ended.</summary>
    Ended,

    /// <summary>Both.</summary>
    All,
}
