# 🛡️ NeoWallet Security Audit & Production Checklist

This document details the enterprise-grade security architecture, compliance standards, and defensive controls implemented across NeoWallet (.NET 8 & Next.js 16).

---

## 1. Transport & Network Security (HSTS & HTTPS)

- [x] **HTTP Strict Transport Security (HSTS):** Enabled in both .NET 8 (`app.UseHsts()`) and Vercel edge (`vercel.json`) with `max-age=63072000; includeSubDomains; preload`.
- [x] **Forced HTTPS Redirection:** All HTTP connections are automatically upgraded to TLS 1.3 / HTTPS.
- [x] **Zero Plaintext Credentials:** All inter-service communications (PostgreSQL, SignalR, Next.js proxy) use encrypted TLS channels.

---

## 2. HTTP Security Headers Matrix

| Header | Production Value | Protection Target |
| :--- | :--- | :--- |
| `Strict-Transport-Security` | `max-age=63072000; includeSubDomains; preload` | Prevents Man-in-the-Middle (MitM) & SSL stripping |
| `X-Frame-Options` | `DENY` | Prevents Clickjacking and UI redressing attacks |
| `X-Content-Type-Options` | `nosniff` | Prevents MIME-type sniffing vulnerabilities |
| `X-XSS-Protection` | `1; mode=block` | Enables legacy browser XSS filters |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Minimizes leakage of sensitive query parameters |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` | Blocks access to unauthorized hardware sensors |

---

## 3. Authentication & Token Lifecycle

- [x] **Short-Lived JWT Access Tokens:** 15-minute expiration (`exp`) with `ClockSkew = TimeSpan.Zero`.
- [x] **Cryptographic Symmetric Signing:** 256-bit+ HMAC-SHA256 symmetric signing keys.
- [x] **Refresh Token Rotation (RTR):** Single-use 7-day refresh tokens; rotating upon each refresh request to neutralize token replay.
- [x] **Secure Cookie Management:** Tokens stored in `SameSite=Lax`, `Secure`, and `HttpOnly` configurations.
- [x] **Two-Factor Authentication (2FA/TOTP):** Time-based One-Time Passwords adhering to RFC 6238.
- [x] **Password Hashing:** PBKDF2 / Argon2 cryptographic key derivation with high iteration counts and cryptographic salts.

---

## 4. Financial & Event Sourcing Integrity

- [x] **Optimistic Concurrency Control (OCC):** Strict version validation on stream updates prevents race conditions and double-spending.
- [x] **Idempotency Headers:** UUIDv4 `Idempotency-Key` mandatory on all mutating endpoints (`deposit`, `withdraw`, `transfer`).
- [x] **Distributed Sagas:** MassTransit state machine orchestrates money transfers across wallets with automatic compensating rollbacks.
- [x] **Cryptographic Hash Chaining:** Audit log entries chained with SHA-256 tamper-evident checksums.

---

## 5. Input Validation & Defense in Depth

- [x] **FluentValidation Pipeline:** All incoming API requests validated before handler execution.
- [x] **Structured ProblemDetails (RFC 7807):** Sanitized error outputs without leaking stack traces or internal server paths.
- [x] **Correlation IDs:** End-to-end `X-Correlation-Id` tracing on every request for real-time auditability.
