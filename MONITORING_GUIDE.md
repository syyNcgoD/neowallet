# 📈 NeoWallet Monitoring & Observability Guide

NeoWallet includes production-grade observability via OpenTelemetry, Prometheus metrics, Serilog structured logging, and health checks.

## 1. Health Checks
- **Root Health Endpoint:** `GET /`
- **Output:**
  ```json
  {
    "application": "NeoWallet Enterprise Distributed Wallet",
    "status": "Healthy",
    "version": "1.0.0",
    "timestampUtc": "2026-08-17T11:00:00Z"
  }
  ```

## 2. Prometheus Metrics
- **Scraping Endpoint:** `GET /metrics`
- **Tracked Metrics:**
  - `neowallet_wallet_created_total`
  - `neowallet_money_deposited_total`
  - `neowallet_money_withdrawn_total`
  - `neowallet_transfers_completed_total`
  - `http_server_request_duration_seconds`

## 3. Structured Logging & Distributed Tracing
- **Serilog Enrichers:** `CorrelationId`, `MachineName`, `ThreadId`, `Environment`.
- **Trace Propagation:** `X-Correlation-Id` header is passed seamlessly from Next.js to .NET Web API and logged with every database query and event emission.
