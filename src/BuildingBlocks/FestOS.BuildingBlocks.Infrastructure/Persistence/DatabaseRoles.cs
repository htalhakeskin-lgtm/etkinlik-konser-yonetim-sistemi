using System.Text.RegularExpressions;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>The database roles and their names (database §4, ADR-0021).</summary>
public static partial class DatabaseRoles
{
    /// <summary>Owns every schema and object; nobody logs in as it.</summary>
    public const string Owner = "festos_owner";

    /// <summary>Applies migrations; a member of <see cref="Owner"/> so the objects it creates belong to the owner.</summary>
    public const string Migrator = "festos_migrator";

    /// <summary>Reads every schema, for manual inspection and later reporting.</summary>
    public const string ReadOnly = "festos_readonly";

    /// <summary>Reads server statistics for the telemetry collector; no table access.</summary>
    public const string Monitor = "festos_monitor";

    /// <summary>The role a module connects with, e.g. <c>festos_booking</c> for the <c>booking</c> schema.</summary>
    public static string ForModule(string schema)
    {
        if (!SchemaName().IsMatch(schema))
        {
            throw new ArgumentException(
                $"'{schema}' is not a valid schema name: lower-case ASCII letters, digits and underscores (naming §5).",
                nameof(schema)
            );
        }

        return "festos_" + schema;
    }

    [GeneratedRegex("^[a-z][a-z0-9_]{0,40}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex SchemaName();
}
