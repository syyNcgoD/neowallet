# 🔗 NeoWallet Frontend & Backend Integration Documentation

## 📌 Architecture Overview

NeoWallet is a high-throughput, bank-grade fintech solution combining:
- **Backend:** .NET 8, Clean Architecture, CQRS (MediatR), Event Sourcing (Marten PostgreSQL), Optimistic Concurrency Control (OCC), MassTransit Saga Orchestrator, JWT with Refresh Token Rotation, TOTP 2FA, OpenTelemetry, Prometheus metrics, and SignalR WebSocket Hubs.
- **Frontend:** Next.js 16 (Turbopack), React 19, Tailwind CSS 4, Shadcn/UI, TanStack Query v5, Axios with 401 token auto-refresh interceptors, SignalR client, Motion animations, Recharts, and Sonner notifications.

---

## 🛠️ Configuration & Environment Variables

Create `frontend/.env.local` (or configure in deployment environment):

```env
# REST API Base URL (.NET Web API)
NEXT_PUBLIC_API_URL=http://localhost:5000/api

# Real-Time SignalR WebSockets Hub
NEXT_PUBLIC_SIGNALR_URL=http://localhost:5000/hubs/wallet
```

---

## 🔐 Authentication & Session Flow

1. **Sign-In (`/sign-in`):**
   - Sends `POST /api/auth/login` with email and password.
   - Stores `neowallet_access_token` and `neowallet_refresh_token` in secure cookies.
   - Decodes claims and initializes `AuthProvider` context.
2. **Sign-Up (`/sign-up`):**
   - Sends `POST /api/auth/register`.
   - Auto-initializes session and redirects to `/dashboard`.
3. **Silent Token Refresh:**
   - Interceptors in `client.ts` detect `401 Unauthorized` responses.
   - Requests `POST /api/auth/refresh-token` in background.
   - Replays original failed request without interrupting user workflow.
4. **Route Protection (`middleware.ts`):**
   - Guards all dashboard routes (`/dashboard`, `/transactions`, `/transfers`, `/accounts`, `/cards`, `/settings`, etc.).
   - Redirects unauthenticated traffic to `/sign-in`.

---

## 📡 Real-Time WebSockets (SignalR)

- **Connection:** Managed by `WalletHubService` with automatic reconnect (`[0s, 2s, 5s, 10s, 30s]`).
- **Group Subscription:** `JoinWalletGroup(walletId)` joins the authenticated user's wallet stream.
- **Live Events:**
  - `BalanceChanged(walletId, newBalance, currency)`: Automatically invalidates React Query cache and notifies user with Sonner toast.
  - `TransactionOccurred(transactionDto)`: Pushes new ledger entry to recent activity and transactions table.

---

## 🚀 How to Run the Entire System

### Step 1: Start Backend Infrastructure
```bash
# In repository root
docker-compose up -d postgres rabbitmq prometheus
```

### Step 2: Run .NET 8 Backend API
```bash
# In repository root
dotnet run --project src/NeoWallet.Api
# Running on http://localhost:5000 (Swagger: http://localhost:5000/swagger)
```

### Step 3: Run Next.js Frontend
```bash
cd frontend
pnpm dev
# Running on http://localhost:3000
```

---

## 🧪 Testing Summary

- **Backend:** 252 tests passing (Domain Unit Tests, Application Unit Tests, Infrastructure Integration Tests, Architecture Tests, Web API Integration Tests).
- **Frontend:** Next.js production build (`pnpm build`) passing 100% with 0 TypeScript/ESLint errors across all 18 routes.
