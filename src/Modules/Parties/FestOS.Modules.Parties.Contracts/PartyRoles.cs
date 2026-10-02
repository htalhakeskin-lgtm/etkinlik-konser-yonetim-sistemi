namespace FestOS.Modules.Parties.Contracts;

/// <summary>
/// The party roles as other modules name them (01 §3.2), the same text the API and the database use, and the
/// rule they apply when they choose a party for a field (BR-PTY-004).
/// </summary>
public static class PartyRoles
{
    /// <summary>The rule a selection breaks when the party is inactive or lacks the role.</summary>
    public const string SelectionRuleCode = "BR-PTY-004";

    /// <summary>Buys technical services.</summary>
    public const string Customer = "customer";

    /// <summary>Rents equipment or sells services to the company.</summary>
    public const string Supplier = "supplier";

    /// <summary>Performs on stage.</summary>
    public const string Artist = "artist";

    /// <summary>Represents artists.</summary>
    public const string Agency = "agency";

    /// <summary>Runs venues.</summary>
    public const string VenueOperator = "venueOperator";
}
