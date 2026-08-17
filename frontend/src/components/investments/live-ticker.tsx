"use client"

import { useMemo } from "react"
import { useQuery } from "@tanstack/react-query"
import { apiClient } from "@/lib/api/client"
import { cn } from "@/lib/utils"
import { holdings, watchlistItems } from "@/data/seed"

type TickerEntry = {
  symbol: string
  price: number
  change: number
}

interface StockQuoteApi {
  symbol: string
  companyName: string
  currentPrice: number
  change: number
  percentChange: number
  highPriceOfDay: number
  lowPriceOfDay: number
  openPriceOfDay: number
  previousClosePrice: number
}

export function LiveTicker() {
  const { data: liveStocks } = useQuery<StockQuoteApi[]>({
    queryKey: ["live-stock-quotes"],
    queryFn: async () => {
      try {
        const res = await apiClient.get<StockQuoteApi[]>("/market/stocks")
        return res.data
      } catch {
        return []
      }
    },
    refetchInterval: 30000,
    staleTime: 20000,
  })

  const entries = useMemo<TickerEntry[]>(() => {
    if (liveStocks && liveStocks.length > 0) {
      return liveStocks.map((s) => ({
        symbol: s.symbol,
        price: s.currentPrice,
        change: s.percentChange,
      }))
    }

    const fromHoldings: TickerEntry[] = holdings.map((h) => ({
      symbol: h.symbol,
      price: h.currentPrice,
      change:
        Math.round(
          ((h.currentPrice - h.avgBuyPrice) / h.avgBuyPrice) * 10000
        ) / 100,
    }))
    const fromWatchlist: TickerEntry[] = watchlistItems.map((w) => ({
      symbol: w.symbol,
      price: w.currentPrice,
      change: w.dayChange,
    }))
    return [...fromHoldings, ...fromWatchlist]
  }, [liveStocks])

  const tickerItems = entries.map((e, i) => {
    const positive = e.change >= 0
    return (
      <span
        key={`${e.symbol}-${i}`}
        className="inline-flex items-center gap-1.5 px-4 text-xs"
      >
        <span className="font-medium">{e.symbol}</span>
        <span className="tabular-nums text-muted-foreground">
          ${e.price.toFixed(2)}
        </span>
        <span
          className={cn(
            "tabular-nums font-medium",
            positive
              ? "text-emerald-600 dark:text-emerald-400"
              : "text-rose-600 dark:text-rose-400"
          )}
        >
          {positive ? "+" : ""}
          {e.change.toFixed(2)}%
        </span>
      </span>
    )
  })

  return (
    <div className="group sticky top-0 z-20 h-10 w-full overflow-hidden border-b bg-background">
      <div className="animate-marquee group-hover:[animation-play-state:paused] absolute flex h-full items-center whitespace-nowrap">
        <div className="flex items-center">
          {tickerItems}
        </div>
        {/* Duplicate for seamless loop */}
        <div className="flex items-center" aria-hidden>
          {tickerItems}
        </div>
      </div>
    </div>
  )
}
