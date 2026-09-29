namespace FestOS.BuildingBlocks.Application.Errors;

/// <summary>The request broke one or more validation rules; the API answers 400 (api §8.2).</summary>
public sealed class ValidationFailedException : Exception
{
    /// <summary>Creates the exception.</summary>
    public ValidationFailedException(IReadOnlyList<ValidationError> errors)
        : base("The request failed validation.")
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
    }

    /// <summary>Every failed rule, in the order the validators reported them.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }
}
