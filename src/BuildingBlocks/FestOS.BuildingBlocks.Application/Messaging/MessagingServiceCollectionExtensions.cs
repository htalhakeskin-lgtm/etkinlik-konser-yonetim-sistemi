using System.Reflection;
using FestOS.BuildingBlocks.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>Registers handlers and validators and wraps the handlers in the decorators (building-blocks §4).</summary>
public static class MessagingServiceCollectionExtensions
{
    private static readonly Type[] HandlerAndValidatorTypes =
    [
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
        typeof(IDomainEventHandler<>),
        typeof(IIntegrationEventHandler<>),
        typeof(IValidator<>),
    ];

    /// <summary>
    /// Registers the command, query, domain and integration event handlers and the validators of one module's
    /// Application assembly. Internal types are included, since handlers are <c>internal sealed</c>.
    /// </summary>
    public static IServiceCollection AddHandlersFrom(this IServiceCollection services, Assembly assembly) =>
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableToAny(HandlerAndValidatorTypes), publicOnly: false)
                .AsImplementedInterfaces(IsHandlerOrValidator)
                .WithScopedLifetime()
        );

    /// <summary>
    /// Wraps every registered handler, outermost first: logging, then validation. Called once, after all
    /// modules have registered their handlers; calling it per module would wrap handlers twice.
    /// </summary>
    public static IServiceCollection DecorateHandlers(this IServiceCollection services)
    {
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(ValidationCommandDecorator<,>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(LoggingCommandDecorator<,>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(ValidationQueryDecorator<,>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(LoggingQueryDecorator<,>));
        return services;
    }

    // Validators also implement IEnumerable<IValidationRule>, which must not become a service.
    private static bool IsHandlerOrValidator(Type type) =>
        type.IsGenericType && HandlerAndValidatorTypes.Contains(type.GetGenericTypeDefinition());
}
