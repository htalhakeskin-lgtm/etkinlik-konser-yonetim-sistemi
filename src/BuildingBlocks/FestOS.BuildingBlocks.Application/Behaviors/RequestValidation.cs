using FestOS.BuildingBlocks.Application.Errors;
using FluentValidation;
using FluentValidation.Results;

namespace FestOS.BuildingBlocks.Application.Behaviors;

/// <summary>Runs every validator registered for a request and throws when any rule fails.</summary>
internal static class RequestValidation
{
    // FluentValidation adds these to every failure; the submitted value must not travel further.
    private static readonly HashSet<string> OmittedPlaceholders = new(StringComparer.Ordinal)
    {
        "PropertyName",
        "PropertyPath",
        "PropertyValue",
    };

    public static async Task ValidateAsync<TRequest>(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators,
        CancellationToken cancellationToken
    )
    {
        List<ValidationError> errors = [];

        foreach (IValidator<TRequest> validator in validators)
        {
            ValidationResult result = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            errors.AddRange(result.Errors.Select(ToError));
        }

        if (errors.Count > 0)
        {
            throw new ValidationFailedException(errors);
        }
    }

    private static ValidationError ToError(ValidationFailure failure) =>
        new(
            failure.PropertyName,
            failure.ErrorCode,
            (failure.FormattedMessagePlaceholderValues ?? [])
                .Where(placeholder => !OmittedPlaceholders.Contains(placeholder.Key))
                .ToDictionary(
                    placeholder => placeholder.Key,
                    object? (placeholder) => placeholder.Value,
                    StringComparer.Ordinal
                )
        );
}
