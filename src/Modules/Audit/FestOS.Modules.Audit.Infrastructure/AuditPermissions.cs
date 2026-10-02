namespace FestOS.Modules.Audit.Infrastructure;

/// <summary>The permissions Audit defines (audit §3, naming §8.1).</summary>
public static class AuditPermissions
{
    /// <summary>Reads the change history.</summary>
    public const string ViewEntries = "Audit.Entries.View";

    /// <summary>All of them, as the module reports them to the Host.</summary>
    public static IReadOnlyCollection<string> All { get; } = [ViewEntries];
}
