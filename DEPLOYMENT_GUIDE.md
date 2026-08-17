# 🚀 NeoWallet Production Deployment Guide

## 1. Cloud Architecture Overview

- **Frontend:** Next.js 16 deployed on **Vercel** with Edge Middleware & Rewrites.
- **Backend:** .NET 8 Web API container deployed on **Railway**.
- **Database:** PostgreSQL 16 on **Railway** with Marten Event Store.

---

## 2. Environment Variables Specification

### Backend (Railway)
```env
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
DATABASE_URL=postgresql://postgres:<PASSWORD>@postgres.railway.internal:5432/railway
Marten__AutoCreateSchemaObjects=true
Jwt__Secret=SuperSecretKeyForNeoWalletEnterpriseDistributedEventSourcedWalletSystem2026!
```

### Frontend (Vercel)
```env
NEXT_PUBLIC_API_URL=/api
NEXT_PUBLIC_SIGNALR_URL=/hubs/wallets
NEXT_PUBLIC_APP_URL=https://frontend-khaki-eta-q0o1goip7w.vercel.app
BACKEND_API_URL=https://neowallet-production.up.railway.app
```

---

## 3. Step-by-Step Deployment

1. **Push code to GitHub:**
   ```bash
   git push origin main
   ```
2. **Deploy Backend to Railway:**
   - Link GitHub repository `syyNcgoD/neowallet`.
   - Add PostgreSQL plugin.
   - Set environment variables as listed above.
3. **Deploy Frontend to Vercel:**
   - Run `npx vercel --prod --yes` from inside `frontend/` directory.
   - Configure custom domain in Vercel settings.
