using FestOS.BuildingBlocks.Application.Paging;
using FluentValidation;

namespace FestOS.Modules.Audit.Infrastructure.Entries;

internal sealed class ListAuditEntriesValidator : AbstractValidator<ListAuditEntriesQuery>
{
    public ListAuditEntriesValidator()
    {
        RuleFor(query => query.Module).MaximumLength(ListAuditEntriesQuery.MaxNameLength);
        RuleFor(query => query.RootType).MaximumLength(ListAuditEntriesQuery.MaxNameLength);
        RuleFor(query => query.To)
            .GreaterThan(query => query.From)
            .When(query => query.From is not null && query.To is not null);
        RuleFor(query => query.Cursor).SetValidator(new CursorRequestValidator());
    }
}
