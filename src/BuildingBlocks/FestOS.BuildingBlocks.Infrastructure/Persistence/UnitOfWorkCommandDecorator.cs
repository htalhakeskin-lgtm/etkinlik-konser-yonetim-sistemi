using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Infrastructure.Messaging;

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
    public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken) =>
        ModuleUnitOfWork.RunAsync(
            ModuleUnitOfWork.ContextFor(services, typeof(TCommand)),
            outbox,
            retryCancellationToken => inner.HandleAsync(command, retryCancellationToken),
            cancellationToken
        );
}
