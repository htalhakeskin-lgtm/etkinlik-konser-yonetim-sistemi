namespace FestOS.Modules.Parties.Contracts;

/// <summary>The permissions Parties defines (parties §7, naming §8.1).</summary>
public static class PartiesPermissions
{
    /// <summary>Lists and opens parties.</summary>
    public const string ViewParties = "Parties.Parties.View";

    /// <summary>Creates parties.</summary>
    public const string CreateParties = "Parties.Parties.Create";

    /// <summary>Changes a party, its contact persons and its representation.</summary>
    public const string EditParties = "Parties.Parties.Edit";

    /// <summary>Deactivates and reactivates parties.</summary>
    public const string DeactivateParties = "Parties.Parties.Deactivate";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } =
    [ViewParties, CreateParties, EditParties, DeactivateParties];
}
