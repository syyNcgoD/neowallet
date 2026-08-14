namespace NeoWallet.Domain.Common;

/// <summary>
/// Represents a domain event that occurs within the aggregate root lifecycle.
/// Domain events are immutable historical facts capturing business occurrences.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier of the specific event occurrence.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// UTC timestamp indicating when the domain event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }

    /// <summary>
    /// Identifier of the aggregate root that emitted this event.
    /// </summary>
    Guid AggregateId { get; }
}
