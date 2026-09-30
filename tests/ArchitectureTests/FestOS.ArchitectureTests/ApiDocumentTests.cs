using System.Text.Json;
using System.Text.RegularExpressions;

namespace FestOS.ArchitectureTests;

/// <summary>
/// Rules checked on the committed OpenAPI documents (docs/08-architecture.md §12.2, api §14.4): the Host's
/// document, and the contract document of the platform's sample endpoints. CI keeps both up to date.
/// </summary>
public sealed partial class ApiDocumentTests
{
    private static readonly string[] ChangingMethods = ["post", "put", "patch", "delete"];

    public static TheoryData<string> Documents => ["festos.json", "contract.json"];

    [Theory]
    [MemberData(nameof(Documents))]
    [Trait("ArchitectureRule", "AT-14")]
    public void Operations_HaveAUniqueNameAModuleTagAndASummary(string document)
    {
        List<string> violations = [];
        HashSet<string> names = new(StringComparer.Ordinal);
        HashSet<string> patterns = new(StringComparer.Ordinal);
        Dictionary<string, string> moduleOfFirstSegment = new(StringComparer.Ordinal);
        HashSet<string> modules = ModulesOf(document);

        foreach ((string path, string method, JsonElement operation) in Operations(document))
        {
            string where = $"{method.ToUpperInvariant()} {path}";
            string? name = operation.TryGetProperty("operationId", out JsonElement id) ? id.GetString() : null;
            if (string.IsNullOrEmpty(name) || !names.Add(name))
            {
                violations.Add($"{where}: missing or repeated operationId '{name}'");
            }

            string[] tags = operation.TryGetProperty("tags", out JsonElement tagList)
                ? [.. tagList.EnumerateArray().Select(tag => tag.GetString() ?? string.Empty)]
                : [];
            if (tags.Length != 1 || !modules.Contains(tags[0]))
            {
                violations.Add($"{where}: needs exactly one tag, a module name, not [{string.Join(", ", tags)}]");
            }
            else if (
                !moduleOfFirstSegment.TryAdd(FirstSegment(path), tags[0])
                && !string.Equals(moduleOfFirstSegment[FirstSegment(path)], tags[0], StringComparison.Ordinal)
            )
            {
                violations.Add(
                    $"{where}: /{FirstSegment(path)} already belongs to {moduleOfFirstSegment[FirstSegment(path)]}"
                );
            }

            if (
                !operation.TryGetProperty("summary", out JsonElement summary)
                || string.IsNullOrWhiteSpace(summary.GetString())
            )
            {
                violations.Add($"{where}: missing summary");
            }

            if (!patterns.Add($"{method} {Parameter().Replace(path, "{}")}"))
            {
                violations.Add($"{where}: another operation has the same address pattern and method");
            }
        }

        violations.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Documents))]
    [Trait("ArchitectureRule", "AT-15")]
    public void ChangingOperations_RequireTheirHeaders(string document)
    {
        List<string> violations = [];

        foreach ((string path, string method, JsonElement operation) in Operations(document))
        {
            if (!ChangingMethods.Contains(method, StringComparer.Ordinal))
            {
                continue;
            }

            string[] headers = RequiredHeaders(operation);
            if (!headers.Contains("Idempotency-Key", StringComparer.Ordinal))
            {
                violations.Add($"{method.ToUpperInvariant()} {path}: Idempotency-Key is not required");
            }

            // PUT, PATCH and DELETE change an existing aggregate. An action POST does too, but the document
            // cannot tell it from a create; its endpoint tests check 428 and 412 (testing §4).
            if (
                !string.Equals(method, "post", StringComparison.Ordinal)
                && !headers.Contains("If-Match", StringComparer.Ordinal)
            )
            {
                violations.Add($"{method.ToUpperInvariant()} {path}: If-Match is not required");
            }
        }

        violations.ShouldBeEmpty();
    }

    private static List<(string Path, string Method, JsonElement Operation)> Operations(string document)
    {
        using var json = JsonDocument.Parse(File.ReadAllText(PathOf(document)));
        List<(string, string, JsonElement)> operations = [];
        foreach (JsonProperty path in json.RootElement.GetProperty("paths").EnumerateObject())
        {
            operations.AddRange(
                path.Value.EnumerateObject()
                    .Where(item => item.Name is "get" or "post" or "put" or "patch" or "delete")
                    .Select(item => (path.Name, item.Name, item.Value.Clone()))
            );
        }

        return operations;
    }

    private static string[] RequiredHeaders(JsonElement operation) =>
        operation.TryGetProperty("parameters", out JsonElement parameters)
            ?
            [
                .. parameters
                    .EnumerateArray()
                    .Where(parameter =>
                        string.Equals(parameter.GetProperty("in").GetString(), "header", StringComparison.Ordinal)
                        && parameter.TryGetProperty("required", out JsonElement required)
                        && required.GetBoolean()
                    )
                    .Select(parameter => parameter.GetProperty("name").GetString() ?? string.Empty),
            ]
            : [];

    // The Host's tags are its modules (src/Modules/{Module}); the contract document uses the sample module.
    private static HashSet<string> ModulesOf(string document) =>
        string.Equals(document, "contract.json", StringComparison.Ordinal)
            ? new(["Sample"], StringComparer.Ordinal)
            : new(
                Directory.GetDirectories(Path.Combine(RepositoryRoot(), "src", "Modules")).Select(Path.GetFileName)!,
                StringComparer.Ordinal
            );

    private static string FirstSegment(string path) =>
        path.StartsWith("/api/v1/", StringComparison.Ordinal) ? path["/api/v1/".Length..].Split('/')[0] : path;

    private static string PathOf(string document) => Path.Combine(RepositoryRoot(), "src", "web", "openapi", document);

    private static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FestOS.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
    }

    [GeneratedRegex(@"\{[^}]+\}", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Parameter();
}
