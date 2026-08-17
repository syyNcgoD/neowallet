# 💎 NeoWallet Enterprise Distributed Wallet System

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Tests](https://img.shields.io/badge/tests-252%2F252%20passing-success)
![Next.js](https://img.shields.io/badge/Next.js-16.2.3-black)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)
![Security](https://img.shields.io/badge/security-HSTS%20%7C%202FA%20%7C%20OCC-red)

An enterprise-grade, distributed, event-sourced multi-currency financial wallet and ledger system built with **.NET 8 Clean Architecture**, **Marten Event Sourcing**, **MassTransit Sagas**, and a modern **Next.js 16 (Turbopack + Tailwind CSS 4)** frontend.

---

## 🌐 Live Production Deployments

- **🚀 Frontend (Vercel Edge):** [https://frontend-khaki-eta-q0o1goip7w.vercel.app](https://frontend-khaki-eta-q0o1goip7w.vercel.app)
- **🏦 Backend API (Railway Cloud):** [https://neowallet-production.up.railway.app](https://neowallet-production.up.railway.app)
- **📡 Real-Time WebSockets (SignalR):** `wss://neowallet-production.up.railway.app/hubs/wallets`
- **📂 GitHub Repository:** [https://github.com/syyNcgoD/neowallet](https://github.com/syyNcgoD/neowallet)

---

## 📚 Project Documentation

- [ARCHITECTURE.md](file:///d:/NeoWallet/ARCHITECTURE.md) - System architecture, CQRS & Event Sourcing
- [SECURITY_CHECKLIST.md](file:///d:/NeoWallet/SECURITY_CHECKLIST.md) - Security audit, HSTS, XSS/CSRF, Token rotation
- [API_REFERENCE.md](file:///d:/NeoWallet/API_REFERENCE.md) - REST API endpoints and payload specifications
- [DEPLOYMENT_GUIDE.md](file:///d:/NeoWallet/DEPLOYMENT_GUIDE.md) - Production deployment steps
- [MONITORING_GUIDE.md](file:///d:/NeoWallet/MONITORING_GUIDE.md) - Observability, OpenTelemetry & Prometheus
- [TROUBLESHOOTING.md](file:///d:/NeoWallet/TROUBLESHOOTING.md) - Common issues and recovery runbooks

---

## 🛠️ Tech Stack

### Backend
- **Framework:** .NET 8 Web API (C# 12)
- **Architecture:** Clean Architecture + CQRS + Event Sourcing
- **Event Store & Projections:** Marten 8.x + PostgreSQL 16 (JSONB)
- **Message Bus & Sagas:** MassTransit 8.3.6 (Distributed Transfer State Machine)
- **Real-Time Push:** ASP.NET Core SignalR
- **Observability:** OpenTelemetry + Prometheus + Serilog

### Frontend
- **Framework:** Next.js 16 (App Router + Turbopack)
- **Styling:** Tailwind CSS 4 + Shadcn UI + Base UI
- **State & Real-Time:** TanStack Query v5 + `@microsoft/signalr`
- **Animations:** Motion + Three.js / React Three Fiber

---

## 🧪 Automated Testing Suite (252 / 252 Passing)

```bash
dotnet test
```

- **Domain Unit Tests:** 152 passing
- **Application Unit Tests:** 25 passing
- **Infrastructure Integration Tests:** 60 passing
- **Architecture Validation Tests:** 5 passing
- **Web API Integration Tests:** 10 passing