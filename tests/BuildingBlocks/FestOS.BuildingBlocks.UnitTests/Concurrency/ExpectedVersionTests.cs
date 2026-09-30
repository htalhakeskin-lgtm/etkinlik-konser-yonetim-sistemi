using FestOS.BuildingBlocks.Application.Concurrency;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Entities;
using FestOS.BuildingBlocks.Domain.Events;

namespace FestOS.BuildingBlocks.UnitTests.Concurrency;

[Trait("Rule", "BR-SYS-011")]
public sealed class ExpectedVersionTests
{
    [Fact]
    public void EnsureMatches_WhenTheAggregateIsAtTheExpectedVersion_Passes()
    {
        var expected = new ExpectedVersion();
        expected.Set(7);

        Should.NotThrow(() => expected.EnsureMatches(new VersionedAggregate(7)));
    }

    [Fact]
    public void EnsureMatches_WhenSomeoneChangedTheAggregate_ThrowsAConcurrencyConflict()
    {
        var expected = new ExpectedVersion();
        expected.Set(7);

        ConcurrencyConflictException conflict = Should.Throw<ConcurrencyConflictException>(() =>
            expected.EnsureMatches(new VersionedAggregate(8))
        );
        conflict.Message.ShouldContain("version 8");
        conflict.Message.ShouldContain("version 7");
    }

    [Fact]
    public void EnsureMatches_WithoutAnExpectedVersion_ComparesNothing() =>
        Should.NotThrow(() => new ExpectedVersion().EnsureMatches(new VersionedAggregate(3)));

    [Fact]
    public void Set_Twice_Throws()
    {
        var expected = new ExpectedVersion();
        expected.Set(1);

        Should.Throw<InvalidOperationException>(() => expected.Set(2));
    }

    private sealed record VersionedAggregate(int Version) : IAggregateRoot
    {
        public IReadOnlyCollection<IDomainEvent> DomainEvents => [];

        public DateTimeOffset CreatedAt => default;

        public Guid CreatedBy => default;

        public DateTimeOffset UpdatedAt => default;

        public Guid UpdatedBy => default;

        public IReadOnlyList<IDomainEvent> DequeueDomainEvents() => [];
    }
}
