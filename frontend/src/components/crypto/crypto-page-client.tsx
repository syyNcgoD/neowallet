"use client"

import * as React from "react"
import { useQuery } from "@tanstack/react-query"
import { apiClient } from "@/lib/api/client"
import { MyBalance } from "@/components/crypto/my-balance"
import { TopCoins } from "@/components/crypto/top-coins"
import { MyPortfolio } from "@/components/crypto/my-portfolio"
import { CoinInsight } from "@/components/crypto/coin-insight"
import { TradeForm } from "@/components/crypto/trade-form"
import { MarketOverview } from "@/components/crypto/market-overview"
import { cryptoCoins } from "@/data/seed"

export type CryptoPrices = Record<string, number>

interface CryptoCoinApi {
  id: string
  symbol: string
  name: string
  currentPriceUsd: number
  change24hPercent: number
  high24h: number
  low24h: number
  volume24h: number
}

export function CryptoPageClient() {
  const [selectedCoin, setSelectedCoin] = React.useState("btc")

  // Query live real CoinGecko prices from backend
  const { data: liveCrypto } = useQuery<CryptoCoinApi[]>({
    queryKey: ["live-crypto-prices"],
    queryFn: async () => {
      try {
        const res = await apiClient.get<CryptoCoinApi[]>("/market/crypto")
        return res.data
      } catch {
        // Direct public CoinGecko API fallback
        const fallbackRes = await fetch(
          "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin,ethereum,solana,ripple,cardano,dogecoin,polkadot&vs_currencies=usd&include_24hr_change=true"
        )
        if (fallbackRes.ok) {
          const json = await fallbackRes.json()
          return [
            { id: "btc", symbol: "BTC", name: "Bitcoin", currentPriceUsd: json.bitcoin?.usd || 94250, change24hPercent: json.bitcoin?.usd_24h_change || 2.4, high24h: 0, low24h: 0, volume24h: 0 },
            { id: "eth", symbol: "ETH", name: "Ethereum", currentPriceUsd: json.ethereum?.usd || 2780, change24hPercent: json.ethereum?.usd_24h_change || 1.8, high24h: 0, low24h: 0, volume24h: 0 },
            { id: "sol", symbol: "SOL", name: "Solana", currentPriceUsd: json.solana?.usd || 188, change24hPercent: json.solana?.usd_24h_change || 4.2, high24h: 0, low24h: 0, volume24h: 0 },
            { id: "xrp", symbol: "XRP", name: "Ripple", currentPriceUsd: json.ripple?.usd || 0.58, change24hPercent: json.ripple?.usd_24h_change || -0.5, high24h: 0, low24h: 0, volume24h: 0 },
            { id: "ada", symbol: "ADA", name: "Cardano", currentPriceUsd: json.cardano?.usd || 0.42, change24hPercent: json.cardano?.usd_24h_change || 1.1, high24h: 0, low24h: 0, volume24h: 0 }
          ]
        }
        return []
      }
    },
    refetchInterval: 15000,
    staleTime: 10000,
  })

  // Build initial prices from seed data or live API
  const prices = React.useMemo<CryptoPrices>(() => {
    const p: CryptoPrices = {}
    for (const coin of cryptoCoins) {
      p[coin.id] = coin.price
    }
    if (liveCrypto && liveCrypto.length > 0) {
      for (const item of liveCrypto) {
        const key = item.symbol.toLowerCase()
        p[key] = item.currentPriceUsd
      }
    }
    return p
  }, [liveCrypto])

  const originalPrices = React.useMemo<CryptoPrices>(() => {
    const orig: CryptoPrices = {}
    for (const coin of cryptoCoins) {
      orig[coin.id] = coin.price
    }
    return orig
  }, [])

  return (
    <div className="grid gap-4 px-4 pb-6 lg:grid-cols-12">
      {/* Row 1 */}
      <MyBalance prices={prices} />
      <TopCoins
        prices={prices}
        originalPrices={originalPrices}
        selectedCoin={selectedCoin}
        onSelectCoin={setSelectedCoin}
      />

      {/* Row 2 */}
      <MyPortfolio
        prices={prices}
        originalPrices={originalPrices}
        selectedCoin={selectedCoin}
        onSelectCoin={setSelectedCoin}
      />
      <CoinInsight selectedCoin={selectedCoin} prices={prices} />

      {/* Row 3 */}
      <TradeForm prices={prices} />
      <MarketOverview
        prices={prices}
        originalPrices={originalPrices}
        selectedCoin={selectedCoin}
        onSelectCoin={setSelectedCoin}
      />
    </div>
  )
}
