namespace FestOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>The collations queries name (database §13).</summary>
public static class Collations
{
    /// <summary>
    /// ICU's Turkish order, for sorting what users read, e.g.
    /// <c>EF.Functions.Collate(user.FullName, Collations.Turkish)</c>; never used in an index.
    /// </summary>
    public const string Turkish = "tr-x-icu";
}
