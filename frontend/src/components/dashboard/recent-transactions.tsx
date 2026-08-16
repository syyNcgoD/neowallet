"use client";

import Image from "next/image";
import Link from "next/link";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { recentTransactions } from "@/data/seed";
import {
  MoreHorizontalIcon,
  ChevronRightIcon,
  ArrowUpRightIcon,
  ArrowDownLeftIcon,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useAuth } from "@/contexts/auth-context";
import { useWalletTransactions } from "@/hooks/use-wallets";

const categoryColors: Record<string, string> = {
  Entertainment: "bg-purple-100 text-purple-700 dark:bg-purple-950 dark:text-purple-400",
  Technology: "bg-blue-100 text-blue-700 dark:bg-blue-950 dark:text-blue-400",
  Income: "bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400",
  Design: "bg-pink-100 text-pink-700 dark:bg-pink-950 dark:text-pink-400",
  "AI Tools": "bg-orange-100 text-orange-700 dark:bg-orange-950 dark:text-orange-400",
  Productivity: "bg-sky-100 text-sky-700 dark:bg-sky-950 dark:text-sky-400",
  Deposit: "bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400",
  Withdraw: "bg-amber-100 text-amber-700 dark:bg-amber-950 dark:text-amber-400",
  TransferIn: "bg-emerald-100 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-400",
  TransferOut: "bg-rose-100 text-rose-700 dark:bg-rose-950 dark:text-rose-400",
};

export function RecentTransactions() {
  const { user } = useAuth();
  const { data: liveTransactions, isLoading } = useWalletTransactions(user?.id);

  const hasLiveTransactions = liveTransactions && liveTransactions.length > 0;

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-4">
        <div className="flex items-center gap-2">
          <CardTitle className="text-base font-semibold">
            Recent Transactions
          </CardTitle>
          {hasLiveTransactions && (
            <Badge variant="outline" className="text-[10px] text-emerald-500 border-emerald-500/30">
              Live Event Sourced
            </Badge>
          )}
        </div>
        <Button variant="outline" size="sm" className="h-8 gap-1 text-xs" render={<Link href="/transactions" />}>
          See All
          <ChevronRightIcon className="size-3" />
        </Button>
      </CardHeader>
      <CardContent>
        <div className="overflow-x-auto">
          <div className="min-w-[600px] space-y-1">
            {/* Header */}
            <div className="grid grid-cols-[1fr_140px_100px_120px_32px] gap-4 border-b pb-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">
              <span>Merchant / Type</span>
              <span className="hidden sm:inline">Transaction ID</span>
              <span className="text-right">Amount</span>
              <span className="hidden md:inline">Date</span>
              <span />
            </div>

            {/* Live Rows */}
            {hasLiveTransactions ? (
              liveTransactions.slice(0, 6).map((tx) => {
                const isPositive = tx.type === "Deposit" || tx.type === "TransferIn";
                return (
                  <div
                    key={tx.id}
                    className="group grid grid-cols-[1fr_140px_100px_120px_32px] items-center gap-4 rounded-lg py-2.5 transition-colors hover:bg-muted/50"
                  >
                    <div className="flex items-center gap-3">
                      <div className={cn("flex size-9 shrink-0 items-center justify-center rounded-lg", isPositive ? "bg-emerald-500/10 text-emerald-500" : "bg-muted text-muted-foreground")}>
                        {isPositive ? <ArrowDownLeftIcon className="size-4" /> : <ArrowUpRightIcon className="size-4" />}
                      </div>
                      <div className="min-w-0">
                        <p className="truncate text-sm font-medium">{tx.description || tx.type}</p>
                        <Badge
                          variant="secondary"
                          className={cn(
                            "mt-0.5 h-5 rounded-md px-1.5 text-[10px] font-medium",
                            categoryColors[tx.type] || "bg-muted text-muted-foreground"
                          )}
                        >
                          {tx.type}
                        </Badge>
                      </div>
                    </div>

                    <span className="hidden font-mono text-xs text-muted-foreground sm:inline">
                      {tx.id.slice(0, 8)}...
                    </span>

                    <span
                      className={cn(
                        "text-right text-sm font-semibold tabular-nums",
                        isPositive
                          ? "text-emerald-600 dark:text-emerald-400"
                          : "text-foreground"
                      )}
                    >
                      {isPositive ? "+" : "-"}${Math.abs(tx.amount).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                    </span>

                    <span className="hidden text-xs text-muted-foreground md:inline">
                      {new Date(tx.occurredAtUtc).toLocaleDateString()}
                    </span>

                    <Button
                      variant="ghost"
                      size="icon"
                      className="size-8 opacity-0 transition-opacity group-hover:opacity-100"
                    >
                      <MoreHorizontalIcon className="size-4" />
                    </Button>
                  </div>
                );
              })
            ) : (
              recentTransactions.map((tx) => (
                <div
                  key={tx.id}
                  className="group grid grid-cols-[1fr_140px_100px_120px_32px] items-center gap-4 rounded-lg py-2.5 transition-colors hover:bg-muted/50"
                >
                  <div className="flex items-center gap-3">
                    <Image
                      src={tx.logo}
                      alt={tx.merchant}
                      width={36}
                      height={36}
                      className="size-9 shrink-0 rounded-lg object-contain"
                      unoptimized
                    />
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium">{tx.merchant}</p>
                      <Badge
                        variant="secondary"
                        className={cn(
                          "mt-0.5 h-5 rounded-md px-1.5 text-[10px] font-medium",
                          categoryColors[tx.category]
                        )}
                      >
                        {tx.category}
                      </Badge>
                    </div>
                  </div>

                  <span className="hidden font-mono text-xs text-muted-foreground sm:inline">
                    {tx.transactionId}
                  </span>

                  <span
                    className={cn(
                      "text-right text-sm font-semibold tabular-nums",
                      tx.amount > 0
                        ? "text-emerald-600 dark:text-emerald-400"
                        : "text-foreground"
                    )}
                  >
                    {tx.amount > 0 ? "+" : ""}$
                    {Math.abs(tx.amount).toLocaleString("en-US", {
                      minimumFractionDigits: 2,
                    })}
                  </span>

                  <span className="hidden text-xs text-muted-foreground md:inline">{tx.date}</span>

                  <Button
                    variant="ghost"
                    size="icon"
                    className="size-8 opacity-0 transition-opacity group-hover:opacity-100"
                  >
                    <MoreHorizontalIcon className="size-4" />
                  </Button>
                </div>
              ))
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
