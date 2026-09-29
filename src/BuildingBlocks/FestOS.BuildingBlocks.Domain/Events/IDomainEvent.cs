namespace FestOS.BuildingBlocks.Domain.Events;

/// <summary>
/// Something that happened inside an aggregate. Handled within the same module and transaction;
/// handlers may turn it into an integration event for other modules (docs/05-module-map.md §7).
/// </summary>
/// <remarks>Names end with <c>DomainEvent</c> (AT-06, naming §4.2).</remarks>
public interface IDomainEvent;
