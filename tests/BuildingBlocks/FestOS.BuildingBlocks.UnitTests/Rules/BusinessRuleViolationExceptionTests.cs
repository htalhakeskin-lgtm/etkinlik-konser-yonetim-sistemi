using FestOS.BuildingBlocks.Domain.Rules;

namespace FestOS.BuildingBlocks.UnitTests.Rules;

public sealed class BusinessRuleViolationExceptionTests
{
    [Fact]
    public void Constructor_WithoutKindOrParameters_DefaultsToConstraintWithNoParameters()
    {
        var exception = new BusinessRuleViolationException("SAMPLE-001", "Sample rule violated.");

        exception.RuleCode.ShouldBe("SAMPLE-001");
        exception.Message.ShouldBe("Sample rule violated.");
        exception.Kind.ShouldBe(RuleKind.Constraint);
        exception.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithKindAndParameters_KeepsThem()
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { ["performedBy"] = "Sample User" };

        var exception = new BusinessRuleViolationException(
            "SAMPLE-002",
            "Only the owner may do this.",
            RuleKind.Authorization,
            parameters
        );

        exception.Kind.ShouldBe(RuleKind.Authorization);
        exception.Parameters["performedBy"].ShouldBe("Sample User");
    }

    [Fact]
    public void Constructor_WithBlankRuleCode_Throws() =>
        Should.Throw<ArgumentException>(() => new BusinessRuleViolationException(" ", "No rule."));
}
