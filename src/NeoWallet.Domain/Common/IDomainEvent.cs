namespace NeoWallet.Domain.Common;
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
    Guid AggregateId { get; }
}
