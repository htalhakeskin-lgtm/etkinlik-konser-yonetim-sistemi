using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The innermost command decorator (building-blocks §4, step 7): runs the handler and the save in one
/// transaction of the command's module, inside the retry strategy (database §11.4).
/// </summary>
internal sealed class UnitOfWorkCommandDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IServiceProvider services,
    Outbox outbox
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        string module =
            ModuleNames.Of(typeof(TCommand))
            ?? throw new InvalidOperationException(
                $"{typeof(TCommand).Name} is not in a module namespace (FestOS.Modules.{{Module}}.…)."
            );
        ModuleDbContext context = services.GetRequiredKeyedService<ModuleDbContext>(module);

        return await context
            .Database.CreateExecutionStrategy()
            .ExecuteAsync(
                async retryCancellationToken =>
                {
                    // A retry starts from a clean context and runs the handler again, so it reloads its data.
                    context.ChangeTracker.Clear();
                    outbox.Clear();
                    await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(
                        retryCancellationToken
                    );

                    TResult result = await inner.HandleAsync(command, retryCancellationToken);
                    await context.SaveChangesAsync(retryCancellationToken);
                    await transaction.CommitAsync(retryCancellationToken);
                    return result;
                },
                cancellationToken
            );
    }
}
