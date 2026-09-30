using System.Globalization;
using FestOS.BuildingBlocks.Application.Errors;
using FestOS.BuildingBlocks.Domain.Entities;

namespace FestOS.BuildingBlocks.Application.Concurrency;

/// <summary>
/// The aggregate version the user last saw, taken from the request's <c>If-Match</c> header (api §9). A
/// handler that changes an aggregate calls <see cref="EnsureMatches"/> right after loading it, so a change
/// based on an old copy is rejected instead of silently overwriting someone else's.
/// </summary>
/// <remarks>
/// One per scope. Work no request started, such as jobs and event listeners, has no expected version, so
/// nothing is compared; the unit of work still rejects a concurrent save (database §11.1).
/// </remarks>
public sealed class ExpectedVersion
{
    /// <summary>The version the request expects, or <see langword="null"/> when it came with none.</summary>
    public int? Value { get; private set; }

    /// <summary>Records the version the request expects; set once, by the <c>If-Match</c> filter.</summary>
    public void Set(int version)
    {
        if (Value is not null)
        {
            throw new InvalidOperationException("The expected version of this request is already set.");
        }

        Value = version;
    }

    /// <summary>
    /// Throws <see cref="ConcurrencyConflictException"/> when the loaded aggregate is not at the expected
    /// version.
    /// </summary>
    public void EnsureMatches(IAggregateRoot aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (Value is { } expected && expected != aggregate.Version)
        {
            throw new ConcurrencyConflictException(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{aggregate.GetType().Name} was changed by someone else: it is at version {aggregate.Version}, the request was based on version {expected}."
                )
            );
        }
    }
}
