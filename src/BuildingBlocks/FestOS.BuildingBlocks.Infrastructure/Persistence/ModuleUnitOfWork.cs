using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// One transaction of one module, inside the retry strategy (database §11.4): used for commands, for
/// integration event listeners and by the outbox processor.
/// </summary>
internal static class ModuleUnitOfWork
{
    /// <summary>The context of the module the type belongs to, read from its namespace.</summary>
    public static ModuleDbContext ContextFor(IServiceProvider services, Type type)
    {
        string module =
            ModuleNames.Of(type)
            ?? throw new InvalidOperationException(
                $"{type.Name} is not in a module namespace (FestOS.Modules.{{Module}}.…)."
            );
        return services.GetRequiredKeyedService<ModuleDbContext>(module);
    }

    /// <summary>Runs the work and the save in one transaction and commits.</summary>
    public static Task<TResult> RunAsync<TResult>(
        ModuleDbContext context,
        Outbox outbox,
        Func<CancellationToken, Task<TResult>> work,
        CancellationToken cancellationToken
    ) =>
        context
            .Database.CreateExecutionStrategy()
            .ExecuteAsync(
                async retryCancellationToken =>
                {
                    // A retry starts from a clean context and runs the work again, so it reloads its data.
                    context.ChangeTracker.Clear();
                    outbox.Clear();
                    await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(
                        retryCancellationToken
                    );

                    TResult result = await work(retryCancellationToken);
                    await context.SaveChangesAsync(retryCancellationToken);
                    await transaction.CommitAsync(retryCancellationToken);
                    return result;
                },
                cancellationToken
            );
}
