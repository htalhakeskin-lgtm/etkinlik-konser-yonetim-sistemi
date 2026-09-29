using FestOS.BuildingBlocks.Application.Messaging;
using FestOS.BuildingBlocks.Application.Users;
using Microsoft.Extensions.Logging;

namespace FestOS.BuildingBlocks.Application.Behaviors;

/// <summary>The outermost query decorator: trace span, duration and outcome.</summary>
internal sealed class LoggingQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<LoggingQueryDecorator<TQuery, TResult>> logger
) : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken) =>
        OperationObserver.RunAsync(
            OperationName.Of<TQuery>(),
            () => inner.HandleAsync(query, cancellationToken),
            currentUser,
            timeProvider,
            logger
        );
}
