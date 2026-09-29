using FluentValidation;

namespace FestOS.BuildingBlocks.Application.Paging;

/// <summary>Checks the page number and size; used by list query validators through <c>SetValidator</c>.</summary>
public sealed class PageRequestValidator : AbstractValidator<PageRequest>
{
    /// <summary>Creates the validator.</summary>
    public PageRequestValidator()
    {
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
    }
}
