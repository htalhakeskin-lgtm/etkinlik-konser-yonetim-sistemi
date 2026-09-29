using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Rules;

namespace FestOS.BuildingBlocks.UnitTests.Errors;

public sealed class ErrorCodesTests
{
    [Fact]
    public void Of_ForExpectedOutcomes_ReturnsTheApiCode()
    {
        ErrorCodes.Of(new BusinessRuleViolationException("SAMPLE-001", "Sample rule violated.")).ShouldBe("SAMPLE-001");
        ErrorCodes.Of(new ValidationFailedException([])).ShouldBe("validation");
        ErrorCodes.Of(new NotFoundException("Sample", Guid.CreateVersion7())).ShouldBe("notFound");
        ErrorCodes.Of(new ConcurrencyConflictException("Sample changed.")).ShouldBe("concurrencyConflict");
    }

    [Fact]
    public void Of_ForAnUnexpectedException_ReturnsNull() =>
        ErrorCodes.Of(new InvalidOperationException("Sample failure.")).ShouldBeNull();
}
