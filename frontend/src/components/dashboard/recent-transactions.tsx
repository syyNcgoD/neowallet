"use client";

import Link from "next/link";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  ChevronRightIcon,
  ArrowUpRightIcon,
  ArrowDownLeftIcon,
  ArrowRightLeftIcon,
  PlusIcon,
  ClockIcon,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useWallet } from "@/contexts/wallet-context";
import { useWalletTransactions } from "@/hooks/use-wallets";

export function RecentTransactions() {
  const { activeWallet, deposit, isMutating } = useWallet();
  const { data: liveTransactions = [], isLoading } = useWalletTransactions(activeWallet?.id);

  const hasTransactions = liveTransactions.length > 0;

  return (
    <Card className="bg-card/60 backdrop-blur-sm border-border/40 p-5">
      <CardHeader className="p-0 pb-4 border-b border-border/40 flex flex-row items-center justify-between">
        <div className="flex items-center gap-2">
          <CardTitle className="text-base font-semibold">
            Recent Ledger Activity
          </CardTitle>
          <Badge variant="outline" className="text-[10px] text-emerald-500 border-emerald-500/30">
            Real Event Sourced
          </Badge>
        </div>
        <Button variant="outline" size="sm" className="h-7 gap-1 text-xs" render={<Link href="/transactions" />}>
          View All
          <ChevronRightIcon className="size-3" />
        </Button>
      </CardHeader>

      <CardContent className="p-0 pt-4">
        {isLoading ? (
          <div className="py-8 text-center text-xs text-muted-foreground">
            Reading stream from PostgreSQL Marten...
          </div>
        ) : !hasTransactions ? (
          <div className="py-8 text-center space-y-3">
            <div className="mx-auto size-10 rounded-full bg-muted/60 flex items-center justify-center text-muted-foreground">
              <ClockIcon className="size-5" />
            </div>
            <div>
              <p className="text-sm font-medium">No transactions on this wallet yet</p>
              <p className="text-xs text-muted-foreground mt-0.5">
                Deposit funds to record your first immutable ledger event.
              </p>
            </div>
            <Button
              size="sm"
              onClick={() => deposit(250, "Initial Welcome Deposit")}
              disabled={isMutating || !activeWallet}
              className="text-xs font-medium h-8 bg-emerald-600 hover:bg-emerald-700 text-white"
            >
              <PlusIcon className="size-3.5 mr-1" />
              Deposit $250 Quick Test
            </Button>
          </div>
        ) : (
          <div className="divide-y divide-border/40">
            {liveTransactions.slice(0, 5).map((tx) => {
              const isPositive = tx.type === "Deposit" || tx.type === "TransferIn";
              const isTransfer = tx.type === "TransferIn" || tx.type === "TransferOut";
              const isWithdraw = tx.type === "Withdraw";

              return (
                <div key={tx.id} className="py-3 flex items-center justify-between first:pt-0 last:pb-0">
                  <div className="flex items-center gap-3">
                    <div
                      className={cn(
                        "size-9 rounded-full flex items-center justify-center shrink-0",
                        isPositive
                          ? "bg-emerald-500/10 text-emerald-500 border border-emerald-500/20"
                          : isWithdraw
                          ? "bg-amber-500/10 text-amber-500 border border-amber-500/20"
                          : "bg-primary/10 text-primary border border-primary/20"
                      )}
                    >
                      {isPositive ? (
                        <ArrowDownLeftIcon className="size-4" />
                      ) : isWithdraw ? (
                        <ArrowUpRightIcon className="size-4" />
                      ) : (
                        <ArrowRightLeftIcon className="size-4" />
                      )}
                    </div>

                    <div className="space-y-0.5">
                      <div className="text-xs font-semibold leading-none">
                        {tx.description || (isPositive ? "Deposit" : isWithdraw ? "Withdrawal" : "P2P Transfer")}
                      </div>
                      <div className="text-[10px] text-muted-foreground flex items-center gap-1.5">
                        <span>Ref: {tx.reference || tx.id.slice(0, 8)}</span>
                        <span>•</span>
                        <span>{new Date(tx.occurredAtUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</span>
                      </div>
                    </div>
                  </div>

                  <div className="text-right space-y-0.5">
                    <div
                      className={cn(
                        "text-xs font-bold font-mono",
                        isPositive ? "text-emerald-500" : "text-foreground"
                      )}
                    >
                      {isPositive ? "+" : "-"}${Math.abs(tx.amount).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                    </div>
                    <div className="text-[10px] text-muted-foreground">
                      Bal: ${tx.balanceAfter.toLocaleString()}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
