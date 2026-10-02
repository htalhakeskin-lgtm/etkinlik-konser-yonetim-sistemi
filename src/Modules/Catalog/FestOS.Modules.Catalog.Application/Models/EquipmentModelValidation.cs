using FestOS.Modules.Catalog.Domain.Models;
using FluentValidation;

namespace FestOS.Modules.Catalog.Application.Models;

/// <summary>The field checks a model gets when created or edited (catalog §6).</summary>
internal static class EquipmentModelValidation
{
    public static void ValidateModel<T>(this AbstractValidator<T> validator)
        where T : IEquipmentModelDescription
    {
        validator.RuleFor(model => model.Brand).NotEmpty().MaximumLength(EquipmentModel.BrandMaxLength);
        validator.RuleFor(model => model.Name).NotEmpty().MaximumLength(EquipmentModel.NameMaxLength);
        validator.RuleFor(model => model.TrackingType).IsInEnum();
        validator
            .RuleFor(model => model.Measures.WeightKilograms)
            .GreaterThan(0m)
            .LessThan(10_000_000m)
            .OverridePropertyName("WeightKilograms");
        validator
            .RuleFor(model => model.Measures.PowerWatts)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000)
            .OverridePropertyName("PowerWatts");
        validator
            .RuleFor(model => model.Measures.TransportVolumeCubicMeters)
            .GreaterThan(0m)
            .LessThan(100_000m)
            .OverridePropertyName("TransportVolumeCubicMeters");
    }
}
