# 🏛️ NeoWallet System Architecture

NeoWallet is an enterprise distributed multi-currency financial ledger built with Clean Architecture, CQRS, and Event Sourcing.

```
┌────────────────────────────────────────────────────────┐
│               Frontend (Next.js 16 + Vercel)           │
│  - Tailwind CSS 4, Shadcn UI, Motion, TanStack Query   │
│  - WebSockets / SignalR Provider, Server Rewrites      │
└──────────────────────────┬─────────────────────────────┘
                           │ HTTPS / WSS
┌──────────────────────────▼─────────────────────────────┐
│                 NeoWallet.Api (.NET 8 Web API)         │
│  - JWT & 2FA Auth, Rate Limiting, Security Middlewares │
│  - SignalR WalletHub (/hubs/wallets), Prometheus Metric│
└──────────────────────────┬─────────────────────────────┘
                           │ MediatR & MassTransit
┌──────────────────────────▼─────────────────────────────┐
│              NeoWallet.Application (CQRS / Sagas)      │
│  - Command & Query Handlers, FluentValidation          │
│  - MoneyTransferSaga (Choreography & State Machine)    │
└──────────────────────────┬─────────────────────────────┘
                           │ Domain Events & Repositories
┌──────────────────────────▼─────────────────────────────┐
│         NeoWallet.Infrastructure (Event Sourcing)      │
│  - Marten Event Store & Document Projections           │
│  - Npgsql PostgreSQL Provider, OpenTelemetry Tracing   │
└──────────────────────────┬─────────────────────────────┘
                           │ SQL / JSONB
┌──────────────────────────▼─────────────────────────────┐
│            PostgreSQL 16 Enterprise Database           │
│  - mt_streams, mt_events, mt_doc_walletsummary         │
└────────────────────────────────────────────────────────┘
```

## Architectural Highlights
1. **Clean Architecture Separation:** Domain logic is strictly independent of UI, frameworks, and database persistence.
2. **CQRS (Command Query Responsibility Segregation):** Write operations append immutable domain events (`WalletCreated`, `MoneyDeposited`, `MoneyWithdrawn`). Read operations query high-performance JSONB projections (`WalletSummary`, `TransactionHistory`).
3. **Optimistic Concurrency Control (OCC):** Prevents race conditions and double spending under high concurrency.
4. **MassTransit Saga Orchestration:** Asynchronous multi-step transfers with automatic compensating actions.
5. **SignalR WebSockets:** Real-time push updates to the UI upon ledger mutations.
