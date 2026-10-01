using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Modules;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Application.Passwords;
using FestOS.Modules.Identity.Application.Users;
using FestOS.Modules.Identity.Infrastructure.Authentication;
using FestOS.Modules.Identity.Infrastructure.Passwords;
using FestOS.Modules.Identity.Infrastructure.Sessions;
using FestOS.Modules.Identity.Infrastructure.Users;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
        builder.Services.AddSingleton<ITemporaryPasswordGenerator, TemporaryPasswordGenerator>();
        builder.Services.AddHandlersFrom(typeof(IdentityModuleDefinition).Assembly);
        SessionAuthentication.Register(builder);
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => AuthenticationEndpoints.Map(endpoints);
}
