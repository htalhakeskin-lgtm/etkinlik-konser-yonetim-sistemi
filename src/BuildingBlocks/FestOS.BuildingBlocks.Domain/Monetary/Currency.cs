namespace FestOS.BuildingBlocks.Domain.Monetary;

/// <summary>A currency and the number of decimal places its amounts are rounded to (database §8).</summary>
public readonly record struct Currency
{
    private Currency(string code, int minorUnits)
    {
        Code = code;
        MinorUnits = minorUnits;
    }

    /// <summary>Turkish lira.</summary>
    public static Currency TurkishLira { get; } = new("TRY", 2);

    /// <summary>Euro.</summary>
    public static Currency Euro { get; } = new("EUR", 2);

    /// <summary>US dollar.</summary>
    public static Currency UsDollar { get; } = new("USD", 2);

    /// <summary>The ISO 4217 code, e.g. <c>TRY</c>.</summary>
    public string Code { get; }

    /// <summary>Decimal places of the currency's smallest unit (kuruş, cent).</summary>
    public int MinorUnits { get; }

    /// <summary>The supported currency with the given ISO 4217 code.</summary>
    public static Currency FromCode(string code) =>
        code switch
        {
            "TRY" => TurkishLira,
            "EUR" => Euro,
            "USD" => UsDollar,
            _ => throw new ArgumentException($"Unsupported currency code '{code}'.", nameof(code)),
        };

    /// <inheritdoc />
    public override string ToString() => Code;
}
