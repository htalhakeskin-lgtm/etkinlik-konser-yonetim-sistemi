namespace FestOS.BuildingBlocks.Domain.Monetary;

/// <summary>
/// An amount in a currency. Amounts keep full precision while calculating and are rounded once, per
/// line, to the currency's minor unit, half away from zero (database §8, V-07, V-08).
/// </summary>
public readonly record struct Money(decimal Amount, Currency Currency)
{
    /// <summary>Zero in the given currency.</summary>
    public static Money Zero(Currency currency) => new(0m, currency);

    /// <summary>Adds two amounts of the same currency.</summary>
    public static Money operator +(Money left, Money right) => left.Add(right);

    /// <summary>Subtracts two amounts of the same currency.</summary>
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    /// <summary>Multiplies the amount, e.g. a daily rate by a number of days.</summary>
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    /// <summary>Adds an amount of the same currency.</summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return this with { Amount = Amount + other.Amount };
    }

    /// <summary>Subtracts an amount of the same currency.</summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return this with { Amount = Amount - other.Amount };
    }

    /// <summary>Multiplies the amount without rounding.</summary>
    public Money Multiply(decimal factor) => this with { Amount = Amount * factor };

    /// <summary>Rounds to the currency's minor unit, half away from zero (V-08).</summary>
    public Money Round() =>
        this with
        {
            Amount = decimal.Round(Amount, Currency.MinorUnits, MidpointRounding.AwayFromZero),
        };

    private void EnsureSameCurrency(Money other)
    {
        if (other.Currency != Currency)
        {
            throw new InvalidOperationException($"Cannot combine amounts in {Currency} and {other.Currency}.");
        }
    }
}
