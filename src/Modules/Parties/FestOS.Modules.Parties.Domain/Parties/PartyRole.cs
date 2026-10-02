namespace FestOS.Modules.Parties.Domain.Parties;

/// <summary>A party's relation to the company (01 §3.2); a party may have several (BR-PTY-001).</summary>
public enum PartyRole
{
    /// <summary>Buys technical services.</summary>
    Customer,

    /// <summary>Rents equipment or sells services to the company.</summary>
    Supplier,

    /// <summary>Performs on stage, a person or a group.</summary>
    Artist,

    /// <summary>Represents artists and runs booking talks.</summary>
    Agency,

    /// <summary>Runs one or more venues.</summary>
    VenueOperator,

    /// <summary>Speaks for an organization and has no other role (parties PT-07).</summary>
    Contact,
}
