using FestOS.BuildingBlocks.Domain.Identifiers;

namespace FestOS.BuildingBlocks.UnitTests.Identifiers;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void From_ThroughTheInterface_WrapsTheValue()
    {
        var value = Guid.CreateVersion7();

        SampleId id = Wrap<SampleId>(value);

        id.ShouldBe(new SampleId(value));
        id.Value.ShouldBe(value);
    }

    // Generic code such as the EF and JSON converters creates identifiers this way.
    private static TId Wrap<TId>(Guid value)
        where TId : struct, IStronglyTypedId<TId> => TId.From(value);

    private readonly record struct SampleId(Guid Value) : IStronglyTypedId<SampleId>
    {
        public static SampleId From(Guid value) => new(value);
    }
}
