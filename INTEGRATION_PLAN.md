# 🗺️ NeoWallet Frontend & Backend Integration Plan (INTEGRATION_PLAN.md)

## 📌 Overview
This document maps every page, component, and user interaction in the `shadcn-fintech` Next.js frontend to the corresponding endpoint, WebSocket event, or service in the .NET 8 `NeoWallet` distributed backend.

---

## 🔐 Authentication & Session Flow

| Frontend Route / Action | Backend Endpoint | HTTP Method | Payload / Headers | Description |
| :--- | :--- | :---: | :--- | :--- |
| **Sign In** (`/sign-in`) | `/api/auth/login` | `POST` | `{ email, password }` | Authenticates user; returns JWT `accessToken`, `refreshToken`, and user profile. |
| **Sign Up** (`/sign-up`) | `/api/auth/register` | `POST` | `{ email, password, role: 1 }` | Registers user; returns tokens and initial profile. |
| **Token Refresh** (Interceptors) | `/api/auth/refresh-token` | `POST` | `{ accessToken, refreshToken }` | Auto-refreshes expired access tokens. |
| **Sign Out** | `/api/auth/revoke-token` | `POST` | `{ refreshToken }` | Revokes refresh token on backend and clears client cookies. |
| **2FA Verification** | `/api/auth/2fa/enable` | `POST` | `{ totpCode }` | Validates TOTP code and enables two-factor authentication. |
| **API Keys Management** | `/api/auth/api-keys` | `GET` / `POST` / `DELETE` | `{ name, permissions }` | Manages developer API keys. |

---

## 📱 Page-to-API Endpoint Mapping

### 1. Dashboard (`/dashboard`)
- **Needs:** User's wallets, total balance, recent transactions, spending limits, quick transfer.
- **Backend Integrations:**
  - `GET /api/wallets/{id}/summary`: Fetches live wallet balance, status, currency, OCC version.
  - `GET /api/wallets/{id}/transactions`: Fetches latest 5 transactions for the activity feed.
  - `POST /api/wallets/{id}/transfer`: Executes quick P2P transfer with `Idempotency-Key` header.
  - `POST /api/wallets/{id}/lock` & `/unlock`: Toggles wallet freeze state.
  - `SignalR (WalletHub)`: Real-time balance and transaction push updates.

### 2. Transactions (`/transactions`)
- **Needs:** Full transaction history, filtering by date/type/currency, search, pagination, and CSV export.
- **Backend Integrations:**
  - `GET /api/wallets/{id}/transactions`: Fetches transactions with client-side/query filtering.
  - Client-side CSV generator for exporting transaction records.

### 3. Transfers (`/transfers`)
- **Needs:** Source wallet selector, target wallet input, amount input, idempotency key generation, recent transfer list.
- **Backend Integrations:**
  - `POST /api/wallets/{sourceId}/transfer`: Triggers distributed transfer Saga via MediatR & MassTransit.
  - Validation for currency matching, positive amount, and active wallet status.

### 4. Accounts (`/accounts`)
- **Needs:** Multi-currency wallet cards (USD, EUR, GBP, IRR), create new wallet modal, lock/unlock wallet.
- **Backend Integrations:**
  - `POST /api/wallets`: Creates a new wallet (`{ ownerId, currency }`).
  - `GET /api/wallets/{id}/summary`: Rehydrates individual account summary.
  - `POST /api/wallets/{id}/lock` & `POST /api/wallets/{id}/unlock`: Freezes/unfreezes account.

### 5. Cards (`/cards`)
- **Needs:** Virtual card visualizer, 3D flip card, spending limits, freeze card toggle.
- **Backend Integrations:**
  - Bound to the primary wallet balance and lock/unlock status.

### 6. Analytics (`/analytics`)
- **Needs:** Financial health score, spending categories, daily spending heatmap, monthly revenue comparison.
- **Backend Integrations:**
  - Derived from event streams & transaction history (`MoneyDeposited`, `MoneyWithdrawn`, `MoneyTransferredOut`).

### 7. Budgets (`/budgets`)
- **Needs:** Monthly budget limits, category spending rings, savings goals.
- **Backend Integrations:**
  - Calculated against wallet balances and transaction categories.

### 8. Notifications (`/notifications`)
- **Needs:** Notification list, mark as read, real-time alert toast.
- **Backend Integrations:**
  - `SignalR (WalletHub)` listening to `TransactionOccurred` and `BalanceChanged`.

### 9. Crypto & Investments (`/crypto`, `/investments`)
- **Needs:** Coin prices, market overview, portfolio tracking.
- **Backend Integrations:**
  - Market tracking feeds and mock crypto portfolio values.

### 10. Settings (`/settings`)
- **Needs:** User profile, 2FA setup (TOTP QR code & verification), API key generation, active sessions.
- **Backend Integrations:**
  - `POST /api/auth/2fa/enable`, `POST /api/auth/2fa/disable`, `GET /api/auth/api-keys`, `POST /api/auth/api-keys`.

---

## 📡 Real-time SignalR Integration (`/hubs/wallet`)

- **Hub Endpoint:** `http://localhost:5000/hubs/wallet`
- **Actions:**
  - `JoinWalletGroup(walletId)`: Subscribes the current client connection to wallet updates.
  - `LeaveWalletGroup(walletId)`: Unsubscribes from updates.
- **Inbound Events:**
  - `BalanceChanged(walletId, balance, currency)`: Automatically invalidates React Query cache and animates balance.
  - `TransactionOccurred(transactionDto)`: Triggers Sonner toast notification and adds transaction to the activity list.
