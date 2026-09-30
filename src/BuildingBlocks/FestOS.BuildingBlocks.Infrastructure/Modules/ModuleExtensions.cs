using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Http;
using FestOS.BuildingBlocks.Infrastructure.Idempotency;
using FestOS.BuildingBlocks.Infrastructure.Jobs;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FestOS.BuildingBlocks.Infrastructure.Modules;

/// <summary>Registers the modules and maps their endpoints (08 §5).</summary>
public static class ModuleExtensions
{
    /// <summary>The prefix of every module endpoint (api §3).</summary>
    public const string ApiPrefix = "/api/v1";

    /// <summary>
    /// Registers each module's services in the given order, then wraps all handlers in the decorators
    /// once. The list is the only place a new module is added.
    /// </summary>
    public static IHostApplicationBuilder AddModules(
        this IHostApplicationBuilder builder,
        params IModuleDefinition[] modules
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        EnsureUnique(modules, module => module.Name, "name");
        EnsureUnique(modules, module => module.Schema, "schema");

        foreach (IModuleDefinition module in modules)
        {
            // Rejects schema names that cannot become a role name.
            _ = DatabaseRoles.ForModule(module.Schema);
            module.RegisterServices(builder);
        }

        // Innermost first: the unit of work, then validation and logging around it (building-blocks §4).
        builder.Services.TryDecorate(typeof(ICommandHandler<,>), typeof(UnitOfWorkCommandDecorator<,>));
        builder.Services.DecorateHandlers();
        builder.Services.TryAddScoped<ExpectedVersion>();

        // Every integration event listener runs in its module's unit of work with the inbox (AT-10).
        builder.Services.TryDecorate(typeof(IIntegrationEventHandler<>), typeof(InboxIntegrationEventDecorator<>));
        builder.Services.TryAddSingleton<IEventBus, InProcessEventBus>();
        builder.Services.TryAddSingleton<OutboxProcessor>();
        builder.Services.TryAddSingleton<OutboxSignals>();
        builder.Services.AddMetrics();
        builder.Services.TryAddSingleton<MessagingMetrics>();
        builder
            .Services.AddOptions<MessagingOptions>()
            .Bind(builder.Configuration.GetSection(MessagingOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddSingleton<IValidateOptions<MessagingOptions>, MessagingOptionsValidator>();
        builder.Services.AddHostedService<OutboxDispatcher>();

        // Scheduled jobs (ADR-0013); every module's outbox, inbox and idempotency keys are cleaned up nightly.
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.TryAddSingleton<ScheduledJobRunner>();
        builder.Services.AddHostedService(services => services.GetRequiredService<ScheduledJobRunner>());
        foreach (IModuleDefinition module in modules)
        {
            string moduleName = module.Name;
            builder.Services.AddSingleton<IScheduledJob>(services => new MessagingCleanupJob(
                moduleName,
                services.GetRequiredService<IOptions<MessagingOptions>>(),
                services.GetRequiredService<TimeProvider>()
            ));
            builder.Services.AddSingleton<IScheduledJob>(services => new IdempotencyCleanupJob(
                moduleName,
                services.GetRequiredService<TimeProvider>()
            ));
        }
        builder.Services.AddSingleton(new ModuleCatalog(modules));
        builder.Services.AddSingleton<DatabaseBootstrapper>();
        builder.Services.AddSingleton<DatabaseMigrator>();
        return builder;
    }

    /// <summary>
    /// Maps every module's endpoints on its own group under <see cref="ApiPrefix"/>, tagged with its name.
    /// Every request that changes data needs an idempotency key (api §10).
    /// </summary>
    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        RouteGroupBuilder api = endpoints.MapGroup(ApiPrefix).RequiresIdempotencyKey();

        foreach (IModuleDefinition module in endpoints.ServiceProvider.GetRequiredService<ModuleCatalog>().Modules)
        {
            module.MapEndpoints(api.MapGroup(string.Empty).WithTags(module.Name));
        }

        return endpoints;
    }

    private static void EnsureUnique(IModuleDefinition[] modules, Func<IModuleDefinition, string> key, string what)
    {
        string? duplicate = modules
            .GroupBy(key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Two modules have the {what} '{duplicate}'.");
        }
    }
}
