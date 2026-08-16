"use client";

import React, { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/contexts/auth-context";
import { walletHubService, type BalanceChangedEvent } from "@/lib/signalr/wallet-hub";
import { WALLET_KEYS } from "@/hooks/use-wallets";
import type { TransactionHistoryDto } from "@/types/api";
import { toast } from "sonner";

export function SignalRProvider({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!isAuthenticated || !user?.id) return;

    const walletId = user.id;

    const connectHub = async () => {
      await walletHubService.start();
      await walletHubService.joinWalletGroup(walletId);
    };

    connectHub();

    const unsubBalance = walletHubService.onBalanceChanged((event: BalanceChangedEvent) => {
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(event.walletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.all });
      toast.info(`⚡ Live Balance Update: ${event.currency} ${event.newBalance.toLocaleString()}`);
    });

    const unsubTx = walletHubService.onTransactionOccurred((tx: TransactionHistoryDto) => {
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.transactions(tx.walletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(tx.walletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.all });

      const isPositive = tx.type === "Deposit" || tx.type === "TransferIn";
      toast.success(
        `⚡ Event Sourced: ${tx.type} of ${tx.currency} ${Math.abs(tx.amount).toLocaleString()}`,
        {
          description: tx.description || `New ledger entry at ${new Date().toLocaleTimeString()}`,
        }
      );
    });

    return () => {
      unsubBalance();
      unsubTx();
      walletHubService.leaveWalletGroup(walletId);
    };
  }, [isAuthenticated, user?.id, queryClient]);

  return <>{children}</>;
}
