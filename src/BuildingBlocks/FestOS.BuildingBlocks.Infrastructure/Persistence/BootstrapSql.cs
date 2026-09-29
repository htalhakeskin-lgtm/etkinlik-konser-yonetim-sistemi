using Npgsql;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Runs the bootstrap statements. Role and database names and passwords cannot be query parameters
/// in DDL, so the server builds each statement with <c>format()</c>: <c>%I</c> quotes an identifier,
/// <c>%L</c> a literal. No value is concatenated into SQL in C#.
/// </summary>
internal sealed class BootstrapSql(NpgsqlConnection connection, CancellationToken cancellationToken)
{
    private string? _databaseName;

    public async Task ExecuteAsync(string constantSql)
    {
        await using var command = new NpgsqlCommand(constantSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ExecuteFormattedAsync(string template, params string[] arguments)
    {
        string placeholders = string.Concat(
            Enumerable.Range(2, arguments.Length).Select(index => FormattableString.Invariant($", ${index}"))
        );
        await using var build = new NpgsqlCommand($"SELECT format($1{placeholders})", connection);
        build.Parameters.Add(new NpgsqlParameter<string> { TypedValue = template });
        foreach (string argument in arguments)
        {
            build.Parameters.Add(new NpgsqlParameter<string> { TypedValue = argument });
        }

        string statement = (string)(await build.ExecuteScalarAsync(cancellationToken))!;

#pragma warning disable CA2100 // The statement was built by the server's format() from parameters.
        await using var command = new NpgsqlCommand(statement, connection);
#pragma warning restore CA2100
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> RoleExistsAsync(string role)
    {
        await using var command = new NpgsqlCommand(
            "SELECT EXISTS (SELECT FROM pg_roles WHERE rolname = $1)",
            connection
        );
        command.Parameters.Add(new NpgsqlParameter<string> { TypedValue = role });
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    public async Task<string> DatabaseNameAsync()
    {
        if (_databaseName is null)
        {
            await using var command = new NpgsqlCommand("SELECT current_database()", connection);
            _databaseName = (string)(await command.ExecuteScalarAsync(cancellationToken))!;
        }

        return _databaseName;
    }
}
