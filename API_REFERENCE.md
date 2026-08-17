# 📚 NeoWallet REST API Reference

Base URLs:
- **Production Web API:** `https://neowallet-production.up.railway.app/api`
- **Frontend Edge Proxy:** `https://frontend-khaki-eta-q0o1goip7w.vercel.app/api`

---

## 1. Authentication Endpoints

| Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `POST` | `/api/auth/register` | Register new customer or admin user | ❌ No |
| `POST` | `/api/auth/login` | Authenticate and obtain JWT access token | ❌ No |
| `POST` | `/api/auth/refresh-token` | Rotate refresh token and get fresh JWT | ❌ No |
| `POST` | `/api/auth/2fa/enable` | Enable TOTP two-factor authentication | ✅ Yes |
| `POST` | `/api/auth/2fa/verify` | Verify 2FA TOTP code | ✅ Yes |
| `POST` | `/api/auth/2fa/disable` | Disable 2FA with verification code | ✅ Yes |
| `POST` | `/api/auth/api-keys` | Generate scoped developer API key | ✅ Yes |
| `DELETE` | `/api/auth/api-keys` | Revoke active developer API key | ✅ Yes |

---

## 2. Wallet & Ledger Endpoints

| Method | Route | Description | Headers |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/wallets` | Create new multi-currency wallet stream | `Authorization: Bearer <JWT>` |
| `GET` | `/api/wallets/{id}/summary` | Retrieve optimized wallet summary projection | `Authorization: Bearer <JWT>` |
| `POST` | `/api/wallets/{id}/deposit` | Deposit funds into wallet stream | `Authorization`, `Idempotency-Key` |
| `POST` | `/api/wallets/{id}/withdraw` | Withdraw funds from wallet stream | `Authorization`, `Idempotency-Key` |
| `POST` | `/api/wallets/{id}/transfer` | Execute P2P transfer via Saga | `Authorization`, `Idempotency-Key` |
| `POST` | `/api/wallets/{id}/lock` | Lock wallet for security audit | `Authorization: Bearer <JWT>` |
| `POST` | `/api/wallets/{id}/unlock` | Unlock wallet after verification | `Authorization: Bearer <JWT>` |
| `GET` | `/api/wallets/{id}/transactions`| Fetch immutable ledger history | `Authorization: Bearer <JWT>` |

---

## 3. Real-Time WebSockets (SignalR)

- **Hub Route:** `/hubs/wallets`
- **Events:**
  - `BalanceChanged` `{ walletId, newBalance, currency }`
  - `TransactionOccurred` `{ walletId, transactionId, amount, type, timestamp }`
  - `WalletLocked` `{ walletId, reason }`
  - `WalletUnlocked` `{ walletId }`
