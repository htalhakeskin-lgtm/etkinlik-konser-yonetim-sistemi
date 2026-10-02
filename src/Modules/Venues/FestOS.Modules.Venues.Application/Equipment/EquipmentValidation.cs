using FestOS.Modules.Venues.Domain.Venues;
using FluentValidation;

namespace FestOS.Modules.Venues.Application.Equipment;

/// <summary>The field checks of venue equipment lines and periods (venues §6).</summary>
internal static class EquipmentValidation
{
    /// <summary>The largest quantity.</summary>
    public const int MaxQuantity = 9999;

    public static void ValidateLine<T>(this AbstractValidator<T> validator, Func<T, VenueEquipmentDetails> details)
    {
        validator
            .RuleFor(command => details(command).Quantity)
            .InclusiveBetween(1, MaxQuantity)
            .OverridePropertyName("Quantity");
        validator
            .RuleFor(command => details(command).Description)
            .MaximumLength(VenueEquipment.DescriptionMaxLength)
            .OverridePropertyName("Description");
    }

    public static void ValidatePeriod<T>(this AbstractValidator<T> validator, Func<T, UnavailabilityDetails> details)
    {
        validator
            .RuleFor(command => details(command).Quantity)
            .InclusiveBetween(1, MaxQuantity)
            .OverridePropertyName("Quantity");
        validator
            .RuleFor(command => details(command).Reason)
            .NotEmpty()
            .MaximumLength(VenueEquipmentUnavailability.ReasonMaxLength)
            .OverridePropertyName("Reason");
    }
}
