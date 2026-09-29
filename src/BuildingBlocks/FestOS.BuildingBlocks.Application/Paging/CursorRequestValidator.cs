using FluentValidation;

namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>Checks the cursor and the slice size; used by list query validators through <c>SetValidator</c>.</summary>
public sealed class CursorRequestValidator : AbstractValidator<CursorRequest>
{
    /// <summary>Creates the validator.</summary>
    public CursorRequestValidator()
    {
        RuleFor(request => request.After).MaximumLength(CursorRequest.MaxCursorLength);
        RuleFor(request => request.Limit).InclusiveBetween(1, CursorRequest.MaxLimit);
    }
}
