using System.Collections.Frozen;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using FestOS.Modules.Identity.Application.Passwords;

namespace FestOS.Modules.Identity.Infrastructure.Passwords;

/// <summary>
/// SecLists' common passwords, shipped inside the assembly (identity §6, MIT; see
/// <c>common-passwords.NOTICE.txt</c>) and read once, on first use.
/// </summary>
internal sealed class CommonPasswords : ICommonPasswords
{
    public const string ResourceName = "common-passwords.txt.gz";

    private readonly Lazy<FrozenSet<string>> _passwords = new(Load);

    public bool Contains(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return _passwords.Value.Contains(password.ToLower(CultureInfo.InvariantCulture));
    }

    private static FrozenSet<string> Load()
    {
        using Stream resource =
            typeof(CommonPasswords).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        using var reader = new StreamReader(new GZipStream(resource, CompressionMode.Decompress), Encoding.UTF8);
        var passwords = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            if (line.Length > 0)
            {
                passwords.Add(line);
            }
        }

        return passwords.ToFrozenSet(StringComparer.Ordinal);
    }
}
