namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>
/// Handles one command type. Endpoints receive this interface already wrapped in the decorators, so
/// there is no mediator in between (building-blocks BB-02). Handlers are <c>internal sealed</c> (AT-07).
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>Handles the command.</summary>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
