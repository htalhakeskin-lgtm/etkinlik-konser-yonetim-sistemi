using FestOS.Modules.Venues.Domain.Venues;
using FluentValidation;

namespace FestOS.Modules.Venues.Application.Venues;

/// <summary>The field checks a venue gets when created or edited (venues §6).</summary>
internal static class VenueValidation
{
    public static void ValidateVenue<T>(this AbstractValidator<T> validator)
        where T : IVenueCommand
    {
        validator
            .RuleFor(command => command.Description.Name)
            .NotEmpty()
            .MaximumLength(Venue.NameMaxLength)
            .OverridePropertyName("Name");
        validator
            .RuleFor(command => command.Description.City)
            .NotEmpty()
            .MaximumLength(Venue.CityMaxLength)
            .OverridePropertyName("City");
        validator
            .RuleFor(command => command.Description.Address)
            .NotEmpty()
            .MaximumLength(Venue.AddressMaxLength)
            .OverridePropertyName("Address");
        validator
            .RuleFor(command => command.Description.Capacity)
            .InclusiveBetween(1, 1_000_000)
            .OverridePropertyName("Capacity");
        foreach (
            (string name, Func<T, decimal?> value) in new (string, Func<T, decimal?>)[]
            {
                ("StageWidthMeters", command => command.Description.StageWidthMeters),
                ("StageDepthMeters", command => command.Description.StageDepthMeters),
                ("StageHeightMeters", command => command.Description.StageHeightMeters),
            }
        )
        {
            validator.RuleFor(command => value(command)).GreaterThan(0m).LessThan(1000m).OverridePropertyName(name);
        }

        validator
            .RuleFor(command => command.Description.PowerCapacityAmperes)
            .GreaterThan(0m)
            .LessThan(100_000m)
            .OverridePropertyName("PowerCapacityAmperes");
        validator
            .RuleFor(command => command.Description.LoadingDock)
            .MaximumLength(Venue.LoadingDockMaxLength)
            .OverridePropertyName("LoadingDock");
        validator
            .RuleFor(command => command.Description.TimeZone)
            .NotEmpty()
            .MaximumLength(Venue.TimeZoneMaxLength)
            .Must(zone => TimeZoneInfo.TryFindSystemTimeZoneById(zone, out _))
            .WithErrorCode("invalidValue")
            .OverridePropertyName("TimeZone");
    }
}
