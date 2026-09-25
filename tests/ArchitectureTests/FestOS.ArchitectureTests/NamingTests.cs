using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FestOS.ArchitectureTests;

/// <summary>Naming rules checked on compiled types (docs/08-architecture.md §12.2).</summary>
public sealed partial class NamingTests
{
    private const BindingFlags DeclaredMembers =
        BindingFlags.Public
        | BindingFlags.NonPublic
        | BindingFlags.Instance
        | BindingFlags.Static
        | BindingFlags.DeclaredOnly;

    [Fact]
    [Trait("ArchitectureRule", "AT-12")]
    public void TypeAndMemberNames_Always_UseAsciiLettersDigitsAndUnderscore()
    {
        List<string> violations = [];

        foreach (Type type in FestOSAssemblies.All.SelectMany(assembly => assembly.GetTypes()))
        {
            if (IsCompilerGenerated(type))
            {
                continue;
            }

            if (!AsciiName().IsMatch(StripGenericArity(type.Name)))
            {
                violations.Add(type.FullName ?? type.Name);
            }

            violations.AddRange(
                type.GetMembers(DeclaredMembers)
                    .Where(member => !IsCompilerGenerated(member) && !IsSpecialName(member))
                    .Where(member => !AsciiName().IsMatch(member.Name))
                    .Select(member => type.FullName + "." + member.Name)
            );
        }

        violations.ShouldBeEmpty();
    }

    private static bool IsCompilerGenerated(MemberInfo member) =>
        member.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
        || member.Name.Contains('<', StringComparison.Ordinal);

    private static bool IsSpecialName(MemberInfo member) =>
        member switch
        {
            MethodBase method => method.IsSpecialName || method.IsConstructor,
            FieldInfo field => field.IsSpecialName,
            _ => false,
        };

    private static string StripGenericArity(string name)
    {
        int backtick = name.IndexOf('`', StringComparison.Ordinal);
        return backtick < 0 ? name : name[..backtick];
    }

    [GeneratedRegex("^[A-Za-z0-9_]+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AsciiName();
}
