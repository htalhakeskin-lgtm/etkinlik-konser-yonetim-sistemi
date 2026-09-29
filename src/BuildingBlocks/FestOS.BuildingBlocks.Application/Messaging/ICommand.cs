namespace FestOS.BuildingBlocks.Application.Messaging;

/// <summary>
/// A request to change state, handled by exactly one <see cref="ICommandHandler{TCommand, TResult}"/>.
/// Names end with <c>Command</c> (AT-07).
/// </summary>
/// <typeparam name="TResult">What the caller needs back, e.g. the new identifier or the updated record.</typeparam>
public interface ICommand<TResult>;
