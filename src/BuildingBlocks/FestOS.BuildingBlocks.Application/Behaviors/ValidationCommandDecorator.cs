using FestOS.BuildingBlocks.Application.Messaging;
using FluentValidation;

namespace FestOS.BuildingBlocks.Application.Behaviors;

/// <summary>Validates the command before any transaction is opened (building-blocks §4, step 6).</summary>
internal sealed class ValidationCommandDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IEnumerable<IValidator<TCommand>> validators
) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await RequestValidation.ValidateAsync(command, validators, cancellationToken).ConfigureAwait(false);
        return await inner.HandleAsync(command, cancellationToken).ConfigureAwait(false);
    }
}
