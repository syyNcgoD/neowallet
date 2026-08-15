namespace NeoWallet.Application.Common.Abstractions.Messaging;

public interface IIdempotentCommand
{
    string? IdempotencyKey { get; }
}
