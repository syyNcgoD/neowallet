# 📱 NeoWallet Frontend — Next.js 16 Client

[![Live Demo](https://img.shields.io/badge/Live%20Demo-www.maniiai.ir-emerald?style=for-the-badge&logo=vercel)](https://www.maniiai.ir)
[![Next.js 16](https://img.shields.io/badge/Next.js-16%20Turbopack-black?style=for-the-badge&logo=next.js)](https://nextjs.org/)
[![Tailwind CSS 4](https://img.shields.io/badge/Tailwind-CSS%204-38bdf8?style=for-the-badge&logo=tailwindcss)](https://tailwindcss.com/)
[![React 19](https://img.shields.io/badge/React-19-61dafb?style=for-the-badge&logo=react)](https://react.dev/)

The reactive web client for **NeoWallet**, connected via REST and WebSockets (SignalR) to the .NET 8 Event Sourcing backend.

> 💡 **UI Foundation:**  
> This frontend UI is built upon the open-source design system by [Abderrahim Ghazali (shadcn-fintech)](https://github.com/abderrahimghazali/shadcn-fintech), customized and refactored into a live, event-sourced financial management interface.

---

## 🚀 Key Features

- **Real Multi-Currency Ledger:** Create and switch between USD, EUR, and GBP wallets in real time.
- **Interactive Financial Operations:** Direct deposit modal, P2P transfer saga, and security wallet locking.
- **Live Crypto Rates:** 15s auto-refresh prices powered by CoinGecko API.
- **Live Stock Market:** Real-time equity quotes powered by Finnhub.io.
- **Real-Time SignalR WebSockets:** Instant balance updates and ledger event notifications.
- **Dark/Light Theme:** Built with Tailwind CSS 4 and `next-themes`.

---

## 🛠️ Local Development

```bash
pnpm install
pnpm dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.
