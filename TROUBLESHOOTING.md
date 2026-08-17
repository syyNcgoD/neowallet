# 🔧 NeoWallet Production Troubleshooting Guide

### 1. Issue: 404 NOT_FOUND on Vercel
- **Cause:** Frontend deployed without specifying the `frontend/` directory or missing `BACKEND_API_URL`.
- **Solution:** Deploy directly from `frontend/` using `npx vercel --prod --yes` or ensure Root Directory is set to `frontend` in Vercel settings.

### 2. Issue: 500 Internal Server Error / Database Connection Failed
- **Cause:** Missing `DATABASE_URL` environment variable in Railway.
- **Solution:** Add `DATABASE_URL` in the Railway Variables tab pointing to your PostgreSQL instance.

### 3. Issue: SignalR Real-Time Disconnections
- **Cause:** WebSocket transport blocked by corporate firewall or misconfigured CORS.
- **Solution:** The SignalR client automatically falls back to Long Polling with backoff intervals `[0, 2s, 5s, 10s, 30s]`.
