using CsCheck;
using FestOS.BuildingBlocks.Domain.Monetary;

namespace FestOS.BuildingBlocks.UnitTests.Monetary;

public sealed class MoneyTests
{
    public static TheoryData<decimal, decimal> RoundingCases =>
        new()
        {
            { 2.345m, 2.35m },
            { -2.345m, -2.35m },
            { 2.344m, 2.34m },
            { 2.3450m, 2.35m },
            { 10m, 10m },
        };

    [Theory]
    [MemberData(nameof(RoundingCases))]
    public void Round_ForTurkishLira_RoundsHalfAwayFromZeroToKurus(decimal amount, decimal expected) =>
        new Money(amount, Currency.TurkishLira).Round().Amount.ShouldBe(expected);

    [Fact]
    public void Add_WithSameCurrency_SumsAmounts()
    {
        Money total = new Money(10.5m, Currency.Euro) + new Money(2.25m, Currency.Euro);

        total.ShouldBe(new Money(12.75m, Currency.Euro));
    }

    [Fact]
    public void Add_WithDifferentCurrencies_Throws()
    {
        var lira = new Money(1m, Currency.TurkishLira);
        var euro = new Money(1m, Currency.Euro);

        Should.Throw<InvalidOperationException>(() => lira + euro);
        Should.Throw<InvalidOperationException>(() => lira - euro);
    }

    [Fact]
    public void Multiply_ByAFactor_KeepsFullPrecision()
    {
        Money lineTotal = new Money(33.333m, Currency.TurkishLira) * 3m;

        lineTotal.Amount.ShouldBe(99.999m);
    }

    [Fact]
    public void Round_ForAnyAmount_IsIdempotentAndWithinHalfAKurus() =>
        Gen.Decimal[-1_000_000m, 1_000_000m]
            .Sample(amount =>
            {
                Money rounded = new Money(amount, Currency.TurkishLira).Round();

                return rounded.Round() == rounded && Math.Abs(rounded.Amount - amount) <= 0.005m;
            });

    [Fact]
    public void FromCode_ForASupportedCode_ReturnsTheCurrency()
    {
        Currency.FromCode("TRY").ShouldBe(Currency.TurkishLira);
        Currency.FromCode("USD").MinorUnits.ShouldBe(2);
    }

    [Fact]
    public void FromCode_ForAnUnsupportedCode_Throws() =>
        Should.Throw<ArgumentException>(() => Currency.FromCode("try"));
}
