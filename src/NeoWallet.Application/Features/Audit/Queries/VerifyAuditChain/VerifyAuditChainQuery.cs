using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Features.Audit.Queries.VerifyAuditChain;

public sealed record VerifyAuditChainQuery : IQuery<bool>;

public sealed class VerifyAuditChainQueryHandler : IQueryHandler<VerifyAuditChainQuery, bool>
{
    private readonly IAuditStore _auditStore;

    public VerifyAuditChainQueryHandler(IAuditStore auditStore)
    {
        _auditStore = auditStore;
    }

    public async Task<Result<bool>> Handle(VerifyAuditChainQuery request, CancellationToken cancellationToken)
    {
        return await _auditStore.VerifyChainIntegrityAsync(cancellationToken);
    }
}
