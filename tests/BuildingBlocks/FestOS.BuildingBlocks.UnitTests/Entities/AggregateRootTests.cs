using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.BuildingBlocks.UnitTests.Entities;

public sealed class AggregateRootTests
{
    [Fact]
    public void Raise_WithEvents_KeepsThemInOrder()
    {
        var aggregate = new SampleAggregate(Guid.CreateVersion7());

        aggregate.Rename("first");
        aggregate.Rename("second");

        aggregate
            .DomainEvents.Cast<SampleRenamedDomainEvent>()
            .Select(domainEvent => domainEvent.Name)
            .ShouldBe(["first", "second"]);
    }

    [Fact]
    public void DequeueDomainEvents_AfterRaising_ReturnsEventsAndClearsThem()
    {
        var aggregate = new SampleAggregate(Guid.CreateVersion7());
        aggregate.Rename("first");

        IReadOnlyList<IDomainEvent> dequeued = aggregate.DequeueDomainEvents();

        dequeued.ShouldHaveSingleItem().ShouldBe(new SampleRenamedDomainEvent("first"));
        aggregate.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_ForANewAggregate_StartsAtVersionZero()
    {
        var id = Guid.CreateVersion7();

        var aggregate = new SampleAggregate(id);

        aggregate.Id.ShouldBe(id);
        aggregate.Version.ShouldBe(0);
    }

    private sealed class SampleAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public void Rename(string name) => Raise(new SampleRenamedDomainEvent(name));
    }

    private sealed record SampleRenamedDomainEvent(string Name) : IDomainEvent;
}
