# 🏗️ NeoWallet Production Infrastructure Specification (Phase 0)

This document defines the complete production infrastructure architecture for the **NeoWallet Enterprise Distributed Fintech Platform** and its **Vercel Next.js Frontend Deployment** at `https://neowallet-five.vercel.app`.

---

## 🌐 Production Cloud Topology

```
                               ┌──────────────────────────────────────────────┐
                               │  Vercel Edge Network (Next.js 16 App Router)  │
                               │        https://neowallet-five.vercel.app     │
                               └──────────────────────┬───────────────────────┘
                                                      │
                       HTTPS / WSS (CORS: neowallet-five.vercel.app)
                                                      │
                                                      ▼
                               ┌──────────────────────────────────────────────┐
                               │    Production .NET 8 Backend API Cluster     │
                               │     (Railway / Azure / Render Container)     │
                               │        https://api.neowallet.com             │
                               └───────┬──────────────┬───────────────┬───────┘
                                       │              │               │
            ┌──────────────────────────┘              │               └──────────────────────────┐
            ▼                                         ▼                                          ▼
┌─────────────────────────┐       ┌─────────────────────────┐        ┌─────────────────────────┐
│     PostgreSQL 16       │       │    Redis Cache / OCC    │        │   RabbitMQ Saga Broker  │
│  (Neon.tech / Supabase) │       │   (Upstash Serverless)  │        │   (CloudAMQP Cluster)   │
│  • Marten Event Store   │       │   • Distributed Locks   │        │   • P2P Transfer Sagas  │
│  • Inline CQRS Proj.    │       │   • Token Blacklist     │        │   • Gateway Settlement  │
│  • SHA-512 Audit Ledger │       │   • Rate Limiting       │        │   • Compensating Acts   │
└─────────────────────────┘       └─────────────────────────┘        └─────────────────────────┘
```

---

## ☁️ Cloud Services Matrix

| Service Role | Provider (Recommended) | Tier | Connection / Endpoint Format |
| :--- | :--- | :--- | :--- |
| **Relational & Event Store DB** | **Neon.tech / Supabase** (PostgreSQL 16) | Free / Pro | `postgres://[user]:[pwd]@[neon-host].neon.tech/neowallet?sslmode=require` |
| **Distributed Caching & Locks** | **Upstash Redis** | Serverless | `rediss://default:[pwd]@[upstash-endpoint].upstash.io:6379` |
| **Distributed Message Broker** | **CloudAMQP** (RabbitMQ) | Lemur / Free | `amqps://[user]:[pwd]@[cloudamqp-host].rmq.cloudamqp.com/[vhost]` |
| **Backend API Host** | **Railway / Render / Azure App Service** | Container | `https://api.neowallet.com` |
| **Frontend Web Host** | **Vercel** | Edge Network | `https://neowallet-five.vercel.app` |
| **Distributed Tracing & Logs** | **OpenTelemetry + Grafana Cloud / Sentry** | Free | OTLP gRPC endpoint: `https://otlp-gateway.grafana.net:4317` |

---

## 🔐 Environment Variables Blueprint

### 1. Frontend (`frontend/.env.production` & Vercel Dashboard)
```env
# Next.js API Proxy / Backend REST Base URL
NEXT_PUBLIC_API_URL=/api

# Real-Time SignalR WebSockets Hub (Production Backend)
NEXT_PUBLIC_SIGNALR_URL=https://api.neowallet.com/hubs/wallets

# Canonical Production Application URL
NEXT_PUBLIC_APP_URL=https://neowallet-five.vercel.app
```

### 2. Backend (`appsettings.Production.json` & Container Environment)
```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# PostgreSQL Connection String (Neon.tech with SSL required)
ConnectionStrings__Postgres=Host=[neon-host];Port=5432;Database=neowallet;Username=[user];Password=[pwd];SSL Mode=Require;Trust Server Certificate=true;
Marten__ConnectionString=Host=[neon-host];Port=5432;Database=neowallet;Username=[user];Password=[pwd];SSL Mode=Require;Trust Server Certificate=true;
Marten__SchemaName=public
Marten__AutoCreateSchemaObjects=false

# JWT Security Credentials (Minimum 64-char cryptographically random secret)
Jwt__Secret=[GENERATE_STRONG_RANDOM_64_CHAR_PRODUCTION_SECRET]
Jwt__Issuer=NeoWallet.Api
Jwt__Audience=NeoWallet.Client
Jwt__ExpiryMinutes=15
Jwt__RefreshTokenExpiryDays=7

# CORS Restrictions (Strict whitelist for Vercel deployment)
Cors__AllowedOrigins__0=https://neowallet-five.vercel.app
Cors__AllowedOrigins__1=http://localhost:3000

# OpenTelemetry Monitoring
OpenTelemetry__OtlpEndpoint=https://otlp.grafana.net:4317
```

---

## 🛡️ Production Security & Hardening Controls

1. **Strict CORS Policy:** Restricts credentials and WebSocket communication strictly to `https://neowallet-five.vercel.app`.
2. **Authentication Token Lifecycle:** 15-minute access tokens with automatic rotation of refresh tokens stored in secure, `httpOnly`, `SameSite=Lax` cookies.
3. **Optimistic Concurrency Control (OCC):** Marten stream version checks prevent race conditions on high-frequency wallet operations.
4. **Idempotency Verification:** Unique UUIDv4 idempotency keys required for all mutable financial commands (`deposit`, `withdraw`, `transfer`).
5. **Cryptographic Audit Hash Chaining:** Every ledger event is cryptographically hashed using SHA-512 with previous hash chaining to guarantee immutability.

---

## 📋 Phase 0 Verification Checklist

- [x] Production settings blueprint created (`src/NeoWallet.Api/appsettings.Production.json`)
- [x] Production environment variables created (`frontend/.env.production`)
- [x] Complete cloud architecture & connection strings documented (`INFRASTRUCTURE.md`)
- [x] Security, CORS, and token lifecycle parameters configured
