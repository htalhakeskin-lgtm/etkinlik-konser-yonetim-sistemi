using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// Protection against request forgery, in three layers (api §11): <c>SameSite=Strict</c> cookies, the
/// antiforgery token in <c>X-XSRF-TOKEN</c> on every API request that changes data, and rejecting what
/// the browser reports as coming from another site (<c>Sec-Fetch-Site: cross-site</c>).
/// </summary>
public static class CsrfProtection
{
    /// <summary>The header the front end copies the request token into.</summary>
    public const string HeaderName = "X-XSRF-TOKEN";

    /// <summary>The anonymous endpoint that issues a new token.</summary>
    public const string TokenPath = "/api/v1/antiforgery";

    internal const string AntiforgeryCookie = "festos_antiforgery";

    internal const string TokenCookie = "festos_xsrf";

    private const string HostPrefix = "__Host-";

    /// <summary>
    /// Rejects forged API requests that change data. Call it after authentication, since the token is bound
    /// to the signed-in user.
    /// </summary>
    public static WebApplication UseCsrfProtection(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.Use(RejectForgedRequestsAsync);
        return app;
    }

    /// <summary>Maps <c>GET /api/v1/antiforgery</c>, which issues a new token.</summary>
    public static IEndpointRouteBuilder MapAntiforgeryToken(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints
            .MapGet(
                TokenPath,
                (HttpContext context) =>
                {
                    context.IssueAntiforgeryToken();
                    return TypedResults.NoContent();
                }
            )
            .AllowAnonymous()
            // The request wrapper calls it itself; the generated client does not need it.
            .ExcludeFromDescription();
        return endpoints;
    }

    /// <summary>
    /// Issues a new token: the cookie token in an <c>HttpOnly</c> cookie and the request token in a cookie
    /// the front end reads and sends back in <c>X-XSRF-TOKEN</c>. Signing in and <c>/api/v1/me</c> issue one
    /// too, since a token issued before signing in no longer matches the user.
    /// </summary>
    public static void IssueAntiforgeryToken(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AntiforgeryTokenSet tokens = context
            .RequestServices.GetRequiredService<IAntiforgery>()
            .GetAndStoreTokens(context);
        CookieBuilder antiforgeryCookie = context
            .RequestServices.GetRequiredService<IOptions<AntiforgeryOptions>>()
            .Value.Cookie;

        CookieOptions readable = antiforgeryCookie.Build(context);
        readable.HttpOnly = false;
        string name = antiforgeryCookie.Name!.StartsWith(HostPrefix, StringComparison.Ordinal)
            ? HostPrefix + TokenCookie
            : TokenCookie;
        context.Response.Cookies.Append(name, tokens.RequestToken!, readable);
    }

    // Development serves plain HTTP, where a Secure or __Host- cookie cannot be set; every other
    // environment is HTTPS behind Caddy (09 §6), with the __Host- prefix against cookie injection.
    internal static void Configure(AntiforgeryOptions options, bool plainHttp)
    {
        options.HeaderName = HeaderName;
        options.Cookie.Name = plainHttp ? AntiforgeryCookie : HostPrefix + AntiforgeryCookie;
        options.Cookie.SecurePolicy = plainHttp ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";

        // Framing is already forbidden by the content security policy (security §8).
        options.SuppressXFrameOptionsHeader = true;
    }

    private static async Task RejectForgedRequestsAsync(HttpContext context, RequestDelegate next)
    {
        HttpRequest request = context.Request;
        if (
            IdempotencyFilter.ChangesData(request.Method)
            && request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
        )
        {
            if (string.Equals(request.Headers["Sec-Fetch-Site"], "cross-site", StringComparison.OrdinalIgnoreCase))
            {
                throw Rejected("The browser reported that the request comes from another site.");
            }

            if (!await context.RequestServices.GetRequiredService<IAntiforgery>().IsRequestValidAsync(context))
            {
                throw Rejected(
                    $"The request must send a valid antiforgery token in {HeaderName}; GET {TokenPath} issues a new one."
                );
            }
        }

        await next(context);
    }

    private static RequestHeaderException Rejected(string message) =>
        new(
            StatusCodes.Status403Forbidden,
            "csrf-rejected",
            "Request forgery rejected",
            ProblemCodes.CsrfRejected,
            message
        );
}
