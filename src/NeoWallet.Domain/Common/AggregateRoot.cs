namespace NeoWallet.Domain.Common;
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];
    public long Version { get; protected set; } = -1;
    public IReadOnlyCollection<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }
    protected void RaiseEvent(IDomainEvent domainEvent)
    {
        Guard.AgainstNull(domainEvent, nameof(domainEvent));

        When(domainEvent);
        _uncommittedEvents.Add(domainEvent);
        Version++;
    }
    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        Guard.AgainstNull(history, nameof(history));

        foreach (var @event in history)
        {
            When(@event);
            Version++;
        }
    }
    protected abstract void When(IDomainEvent domainEvent);
}
