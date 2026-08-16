# 💎 NeoWallet — Enterprise Distributed Fintech Platform

[![.NET 8 CI/CD](https://github.com/syyNcgoD/neowallet/actions/workflows/ci.yml/badge.svg)](https://github.com/syyNcgoD/neowallet/actions)
[![Next.js 16](https://img.shields.io/badge/Next.js-16.2-black?logo=next.js)](https://nextjs.org/)
[![React 19](https://img.shields.io/badge/React-19.2-blue?logo=react)](https://react.dev/)
[![Tailwind CSS 4](https://img.shields.io/badge/Tailwind-4.2-38bdf8?logo=tailwindcss)](https://tailwindcss.com/)
[![Tests](https://img.shields.io/badge/Tests-252%20Passed-brightgreen.svg)]()
[![Event Sourcing](https://img.shields.io/badge/Event%20Sourcing-Marten%20PostgreSQL-orange)]()

**NeoWallet** is a bank-grade distributed fintech platform built on **Clean Architecture**, **CQRS (MediatR)**, and **Event Sourcing (Marten PostgreSQL)** on the backend, seamlessly connected to a modern **Next.js 16** frontend with **Tailwind CSS 4**, **Shadcn/UI**, and real-time **SignalR WebSockets**.

---

## 🏛️ System Architecture

```
                                  ┌────────────────────────┐
                                  │   Next.js 16 Frontend  │
                                  │ (Turbopack + Tailwind) │
                                  └───────────┬────────────┘
                                              │
                    ┌─────────────────────────┴─────────────────────────┐
                    │ HTTP REST + JWT (Axios)        SignalR WebSockets │
                    ▼                                                   ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                             NeoWallet.Api (.NET 8)                          │
│                                                                             │
│  [AuthController]      [WalletsController]      [Payments]     [WalletHub]  │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
                               MediatR (CQRS)
                                       │
        ┌──────────────────────────────┴──────────────────────────────┐
        ▼                                                             ▼
┌──────────────────────────────┐                       ┌──────────────────────────────┐
│       Command Handlers       │                       │        Query Handlers        │
│   (Deposit, Withdraw, Saga)  │                       │   (Summary, Transactions)    │
└──────────────┬───────────────┘                       └──────────────┬───────────────┘
               │                                                      │
               ▼                                                      ▼
┌──────────────────────────────┐                       ┌──────────────────────────────┐
│  Marten Event Store & OCC    │                       │     Marten Projections       │
│     (Event Sourcing)         │                       │     (Read Model State)       │
└──────────────┬───────────────┘                       └──────────────┬───────────────┘
               │                                                      │
               └───────────────────────┬──────────────────────────────┘
                                       │
                                       ▼
                        ┌──────────────────────────────┐
                        │    PostgreSQL (Ledger DB)    │
                        └──────────────────────────────┘
```

---

## 🚀 Key Features

### 🏦 Backend Core (.NET 8)
- **Domain-Driven Design (DDD):** Strongly typed Value Objects (`Money`, `Currency`, `WalletId`, `OwnerId`, `TransactionId`), business invariants, and immutable event streams.
- **Event Sourcing & OCC:** Marten PostgreSQL event store with strict Optimistic Concurrency Control (detects and handles write collisions).
- **Distributed Saga Orchestrator:** MassTransit state machine managing multi-step P2P transfers and payment gateway deposits with compensating actions.
- **Bank-Grade Security:** JWT token pair with Refresh Token Rotation (RTR), TOTP Two-Factor Authentication, and scoped Developer API Keys.
- **Audit Ledger & Reconciliation:** SHA-512 cryptographic hash chaining preventing tamper attacks, plus periodic reconciliation verifying ledger balances.
- **Observability:** OpenTelemetry distributed tracing, Prometheus metrics export (`/metrics`), Serilog structured logging, and RFC 7807 `ProblemDetails`.

### 💻 Modern Frontend (Next.js 16)
- **11 Interactive Dashboard Pages:** Dashboard, Transactions, Transfers, Cards, Accounts, Analytics, Budgets, Crypto, Investments, Notifications, Settings, Support.
- **Custom JWT Auth System:** Complete email/password authentication, TOTP 2FA prompt, secure cookie storage, and silent token refresh on 401.
- **TanStack Query v5:** Optimized server state caching, optimistic UI updates, and typed mutations.
- **Real-Time SignalR WebSockets:** Instant balance animation and live toast alerts upon transaction execution.
- **Financial Visualizations:** 3D Three.js interactive globe, dynamic Recharts (category donuts, spending heatmaps, monthly revenue comparisons), and 3D card flip visualizer.
- **CSV Data Export:** Instant transaction history export to `.csv`.

---

## 🚦 Getting Started

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) & [pnpm](https://pnpm.io/)
- [Docker & Docker Compose](https://www.docker.com/)

### 2. Start Supporting Infrastructure
```bash
docker-compose up -d postgres rabbitmq prometheus
```

### 3. Run Backend Web API
```bash
dotnet run --project src/NeoWallet.Api
# Running on http://localhost:5000
# Swagger API docs available at http://localhost:5000/swagger
```

### 4. Run Frontend App
```bash
cd frontend
pnpm install
pnpm dev
# Running on http://localhost:3000
```

---

## 🧪 Automated Testing

NeoWallet includes **252 automated tests** across all architectural layers:

```bash
# Run all unit and integration tests
dotnet test --nologo

# Run frontend build verification
cd frontend
pnpm build
```

| Test Suite | Tests | Result |
| :--- | :---: | :---: |
| **NeoWallet.Domain.UnitTests** | 152 | ✅ Passed |
| **NeoWallet.Application.UnitTests** | 25 | ✅ Passed |
| **NeoWallet.Infrastructure.IntegrationTests** | 60 | ✅ Passed |
| **NeoWallet.ArchitectureTests** | 5 | ✅ Passed |
| **NeoWallet.Api.IntegrationTests** | 10 | ✅ Passed |
| **Total Test Coverage** | **252** | **100% Passed** |

---

## 📂 Project Structure

```
NeoWallet/
├── src/
│   ├── NeoWallet.Domain/          # Entities, Value Objects, Domain Events
│   ├── NeoWallet.Application/     # CQRS Commands/Queries, Validators, Sagas
│   ├── NeoWallet.Infrastructure/  # Marten Repositories, Projections, Auth
│   └── NeoWallet.Api/             # Web API Controllers, SignalR Hub, Metrics
├── tests/
│   ├── NeoWallet.Domain.UnitTests/
│   ├── NeoWallet.Application.UnitTests/
│   ├── NeoWallet.Infrastructure.IntegrationTests/
│   ├── NeoWallet.ArchitectureTests/
│   └── NeoWallet.Api.IntegrationTests/
├── frontend/
│   ├── src/
│   │   ├── app/                   # Next.js 16 App Router (18 routes)
│   │   ├── components/            # Shadcn UI & dashboard widgets
│   │   ├── contexts/              # AuthContext & useAuth hook
│   │   ├── hooks/                 # TanStack Query & SignalR hooks
│   │   ├── lib/api/               # Axios client & typed API endpoints
│   │   └── types/                 # DTOs & Domain contract interfaces
│   └── INTEGRATION.md             # Detailed frontend/backend integration guide
├── Dockerfile                     # Multi-stage production container
├── docker-compose.yml             # PostgreSQL, RabbitMQ, Prometheus setup
└── README.md                      # Platform documentation
```

---

## 📄 License
MIT License. Created for high-scale enterprise fintech deployments.