using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.Modules.Riders.Domain.Productions;

/// <summary>Identifies a <see cref="Production"/>.</summary>
public readonly record struct ProductionId(Guid Value) : IStronglyTypedId<ProductionId>
{
    /// <inheritdoc />
    public static ProductionId From(Guid value) => new(value);

    /// <summary>A new identifier.</summary>
    public static ProductionId New() => new(Guid.CreateVersion7());
}
