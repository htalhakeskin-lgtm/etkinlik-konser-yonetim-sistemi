using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Application.Authentication;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Infrastructure.Authentication;
using FestOS.Modules.Identity.Infrastructure.Passwords;
using FestOS.Modules.Identity.Infrastructure.Sessions;
using FestOS.Modules.Identity.Infrastructure.Users;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FestOS.Modules.Identity.Infrastructure;

/// <summary>Registers the Identity module (05 §5.1, identity.md).</summary>
public sealed class IdentityModuleDefinition : IModuleDefinition
{
    /// <summary>The module name.</summary>
    public const string ModuleName = "Identity";

    /// <summary>The database schema.</summary>
    public const string SchemaName = "identity";

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public string Schema => SchemaName;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions => IdentityPermissions.All;

    /// <inheritdoc />
    public void RegisterServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<IdentityDbContext>(ModuleName, SchemaName);
        builder.Services.AddHandlersFrom(typeof(IdentityPermissions).Assembly);
        builder
            .Services.AddOptions<IdentityModuleOptions>()
            .Bind(builder.Configuration.GetSection(IdentityModuleOptions.SectionName))
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<IdentityModuleOptions>, IdentityModuleOptionsValidator>();
        builder.Services.AddSingleton(services => services.GetRequiredService<IOptions<IdentityModuleOptions>>().Value);
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ILoginAttemptLog, LoginAttemptLog>();
        builder.Services.AddScoped<IUserSessions, UserSessions>();
        builder.Services.AddSingleton<ICommonPasswords, CommonPasswords>();
        builder.Services.AddSingleton<LoginRateLimiter>();
        builder.Services.AddScheduledJob<SessionsCleanupJob>();
        builder.Services.AddScheduledJob<LoginAttemptsCleanupJob>();
        builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
        builder.Services.AddSingleton<ITemporaryPasswordGenerator, TemporaryPasswordGenerator>();
        builder.Services.AddHandlersFrom(typeof(IdentityModuleDefinition).Assembly);
        SessionAuthentication.Register(builder);
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        AuthenticationEndpoints.Map(endpoints);
        UserEndpoints.Map(endpoints);
    }
}
