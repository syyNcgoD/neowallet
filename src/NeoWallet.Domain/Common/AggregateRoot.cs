namespace NeoWallet.Domain.Common;

/// <summary>
/// Base class for Event Sourced Aggregate Roots.
/// Maintains the stream version, tracks uncommitted domain events, and provides state rehydration mechanism.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];

    /// <summary>
    /// Aggregate version representing the total sequence of committed events.
    /// Used for Optimistic Concurrency Control.
    /// </summary>
    public long Version { get; protected set; } = -1;

    /// <summary>
    /// Gets the list of newly raised domain events that have not yet been persisted to the event store.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Applies an event to mutate aggregate state and appends it to uncommitted events.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise and apply.</param>
    protected void RaiseEvent(IDomainEvent domainEvent)
    {
        Guard.AgainstNull(domainEvent, nameof(domainEvent));

        When(domainEvent);
        _uncommittedEvents.Add(domainEvent);
        Version++;
    }

    /// <summary>
    /// Clears the uncommitted events once they have been successfully dispatched / stored.
    /// </summary>
    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    /// <summary>
    /// Rehydrates the aggregate state from historical committed events.
    /// </summary>
    /// <param name="history">The chronological stream of past events.</param>
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        Guard.AgainstNull(history, nameof(history));

        foreach (var @event in history)
        {
            When(@event);
            Version++;
        }
    }

    /// <summary>
    /// Dispatches state mutation to specific event handler methods based on event type.
    /// </summary>
    /// <param name="domainEvent">The event being applied.</param>
    protected abstract void When(IDomainEvent domainEvent);
}
