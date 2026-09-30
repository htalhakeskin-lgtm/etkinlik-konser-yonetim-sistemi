using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Infrastructure.Http.Json;
using FestOS.BuildingBlocks.Infrastructure.Idempotency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FestOS.BuildingBlocks.Infrastructure.Http;

/// <summary>
/// The HTTP platform the Host wires up (building-blocks §9, BB-01): Problem Details for every error,
/// and the headers of every API response.
/// </summary>
public static class HttpPlatformExtensions
{
    private const string ApiPrefix = "/api";

    /// <summary>The largest JSON request body (api §5.3).</summary>
    public const long MaxRequestBodyBytes = 1024 * 1024;

    // Browser security headers of the front end's pages (security §8); HSTS is set by Caddy.
    private const string ContentSecurityPolicy =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; "
        + "font-src 'self'; connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; "
        + "frame-ancestors 'none'";

    private const string PermissionsPolicy = "camera=(self), microphone=(), geolocation=(), payment=(), usb=()";

    /// <summary>The server's version, sent in <c>X-App-Version</c> so an old open tab notices a new release (api §12).</summary>
    public static string AppVersion { get; } = ReadAppVersion();

    /// <summary>
    /// Registers the exception handler, the Problem Details writer, the JSON rules, the body limit, the
    /// request's expected version and its idempotency key.
    /// </summary>
    public static IHostApplicationBuilder AddHttpPlatform(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = Customize);
        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        builder.Services.ConfigureHttpJsonOptions(options => ApiJson.Apply(options.SerializerOptions));
        builder.Services.Configure<KestrelServerOptions>(options =>
            options.Limits.MaxRequestBodySize = MaxRequestBodyBytes
        );
        builder.Services.TryAddScoped<ExpectedVersion>();
        builder.Services.TryAddScoped<IdempotencyRequest>();
        return builder;
    }

    /// <summary>
    /// Adds the error handling and the API response headers; call it first, so it covers everything
    /// after it.
    /// </summary>
    public static WebApplication UseHttpPlatform(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.Use(AddResponseHeadersAsync);
        app.Use(BufferRequestBodyAsync);
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }

    // API responses: the server's version, never cached, never sniffed (api §11, §12). Everything
    // else is the front end, which gets the browser security headers (security §8).
    private static Task AddResponseHeadersAsync(HttpContext context, RequestDelegate next)
    {
        bool isApi = context.Request.Path.StartsWithSegments(ApiPrefix, StringComparison.OrdinalIgnoreCase);
        context.Response.OnStarting(() =>
        {
            IHeaderDictionary headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            if (isApi)
            {
                headers["X-App-Version"] = AppVersion;
                headers.CacheControl = "no-store";
            }
            else
            {
                headers.ContentSecurityPolicy = ContentSecurityPolicy;
                headers["Permissions-Policy"] = PermissionsPolicy;
                headers["Referrer-Policy"] = "no-referrer";
                headers["Cross-Origin-Opener-Policy"] = "same-origin";
            }

            return Task.CompletedTask;
        });

        return next(context);
    }

    // The idempotency filter reads the body of a changing API request again for its fingerprint (api §10);
    // the body is at most 1 MB.
    private static Task BufferRequestBodyAsync(HttpContext context, RequestDelegate next)
    {
        if (
            IdempotencyFilter.ChangesData(context.Request.Method)
            && context.Request.Path.StartsWithSegments(ApiPrefix, StringComparison.OrdinalIgnoreCase)
        )
        {
            context.Request.EnableBuffering();
        }

        return next(context);
    }

    // Also shapes the problems written for responses without a body, such as an unknown API address.
    private static void Customize(ProblemDetailsContext context)
    {
        HttpRequest request = context.HttpContext.Request;
        context.ProblemDetails.Instance = $"{request.Method} {request.Path}";
        context.ProblemDetails.Extensions.Remove("requestId");
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.TraceId.ToHexString() ?? context.HttpContext.TraceIdentifier;

        if (!context.ProblemDetails.Extensions.TryGetValue("code", out object? code) || code is null)
        {
            code = context.ProblemDetails.Status switch
            {
                StatusCodes.Status401Unauthorized => ProblemCodes.Unauthorized,
                StatusCodes.Status403Forbidden => ProblemCodes.Forbidden,
                StatusCodes.Status404NotFound => ErrorCodes.NotFound,
                StatusCodes.Status405MethodNotAllowed => ProblemCodes.MethodNotAllowed,
                StatusCodes.Status500InternalServerError => ProblemCodes.InternalError,
                _ => ProblemCodes.MalformedRequest,
            };
            context.ProblemDetails.Extensions["code"] = code;
        }

        // Problems ASP.NET writes itself point to the RFC; ours use the project's URN (api §8.1).
        if (context.ProblemDetails.Type?.StartsWith(ProblemCodes.TypePrefix, StringComparison.Ordinal) != true)
        {
            context.ProblemDetails.Type =
                ProblemCodes.TypePrefix + JsonNamingPolicy.KebabCaseLower.ConvertName((string)code);
        }
    }

    // MinVer's version without the build metadata: "0.1.0" or "0.1.0-alpha.0.12".
    private static string ReadAppVersion()
    {
        string? version = (Assembly.GetEntryAssembly() ?? typeof(HttpPlatformExtensions).Assembly)
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        return version?.Split('+')[0] ?? "0.0.0";
    }
}
