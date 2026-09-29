using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.Application.Behaviors;

/// <summary>The outermost command decorator: trace span, duration and outcome (building-blocks §4, step 5).</summary>
internal sealed class LoggingCommandDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<LoggingCommandDecorator<TCommand, TResult>> logger
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken) =>
        OperationObserver.RunAsync(
            OperationName.Of<TCommand>(),
            () => inner.HandleAsync(command, cancellationToken),
            currentUser,
            timeProvider,
            logger
        );
}
