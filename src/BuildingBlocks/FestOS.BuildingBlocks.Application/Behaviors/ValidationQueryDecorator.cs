using FestOS.BuildingBlocks.Application.Messaging;
using FluentValidation;

namespace FestOS.BuildingBlocks.Application.Behaviors;

/// <summary>Validates the query, e.g. its paging and filter parameters.</summary>
internal sealed class ValidationQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    IEnumerable<IValidator<TQuery>> validators
) : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        await RequestValidation.ValidateAsync(query, validators, cancellationToken).ConfigureAwait(false);
        return await inner.HandleAsync(query, cancellationToken).ConfigureAwait(false);
    }
}
