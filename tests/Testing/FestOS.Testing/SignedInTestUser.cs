using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FestOS.Testing;

/// <summary>
/// An authentication scheme for test hosts without the Identity module: every request is signed in as
/// <see cref="UserId"/>, without permissions, e.g. to reach the notification hub.
/// </summary>
public sealed class SignedInTestUser(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>The scheme's name.</summary>
    public const string SchemeName = "SignedInTestUser";

    /// <summary>The user every request is signed in as.</summary>
    public static readonly Guid UserId = new("00000000-0000-7000-8000-0000000000aa");

    /// <summary>Makes the scheme the default and adds authorization.</summary>
    public static void Register(IServiceCollection services)
    {
        services
            .AddAuthentication(SchemeName)
            .AddScheme<AuthenticationSchemeOptions, SignedInTestUser>(SchemeName, configureOptions: null);
        services.AddAuthorization();
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, UserId.ToString())], SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new(identity), SchemeName)));
    }
}
