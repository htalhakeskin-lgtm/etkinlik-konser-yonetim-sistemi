namespace FestOS.Modules.Riders.Contracts;

/// <summary>The permissions Riders defines (riders §6, naming §8.1).</summary>
public static class RidersPermissions
{
    /// <summary>Lists and opens productions.</summary>
    public const string ViewProductions = "Riders.Productions.View";

    /// <summary>Creates productions.</summary>
    public const string CreateProductions = "Riders.Productions.Create";

    /// <summary>Changes a production's name and description.</summary>
    public const string EditProductions = "Riders.Productions.Edit";

    /// <summary>Deactivates and reactivates productions.</summary>
    public const string DeactivateProductions = "Riders.Productions.Deactivate";

    /// <summary>Reads riders and their versions.</summary>
    public const string ViewRiders = "Riders.Riders.View";

    /// <summary>Saves a new rider version; the technical manager alone has it.</summary>
    public const string EditRiders = "Riders.Riders.Edit";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } =
    [ViewProductions, CreateProductions, EditProductions, DeactivateProductions, ViewRiders, EditRiders];
}
