using FestOS.BuildingBlocks.Application.Users;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

/// <summary>Cookie authentication over server-side sessions (ADR-0011, ADR-0027, identity §6).</summary>
internal static class SessionAuthentication
{
    public const string CookieName = "festos_session";

    public static void Register(IHostApplicationBuilder builder)
    {
        // Development serves plain HTTP, where a Secure or __Host- cookie cannot be set (BB-11).
        bool isPlainHttp = builder.Environment.IsDevelopment();
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();
        builder.Services.TryAddScoped<ICurrentUser, HttpCurrentUser>();
        builder.Services.AddSingleton<SessionTicketStore>();
        // Without a database (the build's OpenAPI run, the architecture tests) the keys stay in memory.
        if (builder.Configuration.GetConnectionString(DatabaseConnections.ConnectionStringName) is not null)
        {
            builder.Services.AddDbContext<DataProtectionKeyStore>(options =>
                options.UseModuleDatabase(
                    DatabaseConnections.ForRole(
                        builder.Configuration,
                        DatabaseRoles.ForModule(IdentityModuleDefinition.SchemaName)
                    ),
                    IdentityModuleDefinition.SchemaName
                )
            );
            builder.Services.AddDataProtection().PersistKeysToDbContext<DataProtectionKeyStore>();
        }
        builder
            .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = isPlainHttp ? CookieName : "__Host-" + CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = isPlainHttp
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.Path = "/";
                options.ExpireTimeSpan = SessionTicketStore.AbsoluteTimeout;
                options.SlidingExpiration = false;

                // An API answers 401 and 403 with Problem Details; it never redirects to a login page (api §11).
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });
        builder
            .Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<SessionTicketStore>((options, store) => options.SessionStore = store);
    }
}
