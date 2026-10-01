namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>
/// Signing in failed; the API answers 401 with the code, e.g. <c>invalidCredentials</c>, or the rule
/// number of a locked account with its values (api §8.3, security §4.1).
/// </summary>
public sealed class AuthenticationFailedException(
    string code,
    string message,
    IReadOnlyDictionary<string, object?>? parameters = null
) : Exception(message)
{
    /// <summary>The code the API returns.</summary>
    public string Code { get; } = code;

    /// <summary>The values for the message, e.g. when a lock ends.</summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; } =
        parameters ?? new Dictionary<string, object?>(StringComparer.Ordinal);
}
