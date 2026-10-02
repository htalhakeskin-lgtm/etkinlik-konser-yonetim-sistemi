using FestOS.Modules.Riders.Domain.RiderVersions;
using FluentValidation;

namespace FestOS.Modules.Riders.Application.Riders;

internal sealed class CreateRiderVersionValidator : AbstractValidator<CreateRiderVersionCommand>
{
    public CreateRiderVersionValidator()
    {
        RuleFor(command => command.Lines).NotEmpty();
        RuleFor(command => command.Lines.Count).LessThanOrEqualTo(RiderVersion.MaxLines).OverridePropertyName("Lines");
        RuleFor(command => command.Note).MaximumLength(RiderVersion.NoteMaxLength);

        // A line key names one line of the version (riders RD-03).
        RuleFor(command => command.Lines)
            .Must(lines =>
                lines.Where(line => line.LineKey.HasValue).Select(line => line.LineKey).Distinct().Count()
                == lines.Count(line => line.LineKey.HasValue)
            )
            .WithErrorCode("invalidValue");
        RuleForEach(command => command.Lines)
            .ChildRules(line =>
            {
                line.RuleFor(details => details.Quantity).InclusiveBetween(1, RiderLine.MaxQuantity);
                line.RuleFor(details => details.Note).MaximumLength(RiderLine.NoteMaxLength);
                line.RuleFor(details => details.EquivalentModelIds.Count)
                    .LessThanOrEqualTo(RiderLine.MaxEquivalents)
                    .OverridePropertyName("EquivalentModelIds");
                line.RuleFor(details => details.Flexibility).IsInEnum();
            });
    }
}
