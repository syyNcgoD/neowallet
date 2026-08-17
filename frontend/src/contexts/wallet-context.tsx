"use client";

import React, { createContext, useContext, useState, useEffect, useCallback } from "react";
import { useAuth } from "./auth-context";
import { walletApi } from "@/lib/api/wallets";
import type { WalletSummaryDto } from "@/types/api";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { v4 as uuidv4 } from "uuid";
import axios from "axios";

interface WalletContextType {
  wallets: WalletSummaryDto[];
  activeWallet: WalletSummaryDto | null;
  setActiveWalletId: (id: string) => void;
  isLoading: boolean;
  isMutating: boolean;
  createWallet: (currency: string) => Promise<void>;
  deposit: (amount: number, description?: string) => Promise<void>;
  withdraw: (amount: number, description?: string) => Promise<void>;
  transfer: (targetWalletId: string, amount: number, description?: string) => Promise<void>;
  toggleLock: (reason?: string) => Promise<void>;
  refresh: () => Promise<void>;
}

const WalletContext = createContext<WalletContextType | undefined>(undefined);

export function WalletProvider({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [activeWalletId, setActiveWalletIdState] = useState<string | null>(null);
  const [isMutating, setIsMutating] = useState(false);

  const userId = user?.id;

  // Fetch real user wallets from live backend
  const { data: userWallets = [], isLoading, refetch } = useQuery<WalletSummaryDto[]>({
    queryKey: ["wallets", "user", userId],
    queryFn: async () => {
      if (!userId) return [];
      try {
        const list = await walletApi.getUserWallets(userId);
        return list;
      } catch {
        return [];
      }
    },
    enabled: Boolean(userId),
    refetchInterval: 10000,
  });

  // Auto-select or remember active wallet
  useEffect(() => {
    if (userWallets.length > 0) {
      if (!activeWalletId || !userWallets.some((w) => w.id === activeWalletId)) {
        setActiveWalletIdState(userWallets[0].id);
      }
    } else {
      setActiveWalletIdState(null);
    }
  }, [userWallets, activeWalletId]);

  const activeWallet = userWallets.find((w) => w.id === activeWalletId) || userWallets[0] || null;

  const setActiveWalletId = useCallback((id: string) => {
    setActiveWalletIdState(id);
  }, []);

  const createWallet = useCallback(
    async (currency: string) => {
      if (!userId) {
        toast.error("Please sign in to create a wallet.");
        return;
      }
      setIsMutating(true);
      try {
        const res = await walletApi.create({ ownerId: userId, currency });
        toast.success(`New ${currency} Wallet created! ID: ${res.id.slice(0, 8)}...`);
        await refetch();
        setActiveWalletIdState(res.id);
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          toast.error(err.response?.data?.detail || "Failed to create wallet.");
        } else {
          toast.error("Failed to create wallet.");
        }
      } finally {
        setIsMutating(false);
      }
    },
    [userId, refetch]
  );

  const deposit = useCallback(
    async (amount: number, description?: string) => {
      if (!activeWallet) {
        toast.error("No active wallet selected.");
        return;
      }
      setIsMutating(true);
      try {
        const res = await walletApi.deposit(
          activeWallet.id,
          {
            amount,
            currency: activeWallet.currency,
            reference: `DEP-${Date.now().toString().slice(-6)}`,
            description: description || `Deposit into ${activeWallet.currency} Wallet`,
          },
          uuidv4()
        );
        toast.success(`Deposited ${res.currency} ${amount.toLocaleString()} successfully!`);
        await refetch();
        queryClient.invalidateQueries({ queryKey: ["wallets", "transactions", activeWallet.id] });
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          toast.error(err.response?.data?.detail || "Deposit failed.");
        } else {
          toast.error("Deposit failed.");
        }
      } finally {
        setIsMutating(false);
      }
    },
    [activeWallet, refetch, queryClient]
  );

  const withdraw = useCallback(
    async (amount: number, description?: string) => {
      if (!activeWallet) {
        toast.error("No active wallet selected.");
        return;
      }
      setIsMutating(true);
      try {
        const res = await walletApi.withdraw(
          activeWallet.id,
          {
            amount,
            currency: activeWallet.currency,
            reference: `WTH-${Date.now().toString().slice(-6)}`,
            description: description || `Withdrawal from ${activeWallet.currency} Wallet`,
          },
          uuidv4()
        );
        toast.success(`Withdrew ${res.currency} ${amount.toLocaleString()} successfully!`);
        await refetch();
        queryClient.invalidateQueries({ queryKey: ["wallets", "transactions", activeWallet.id] });
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          toast.error(err.response?.data?.detail || "Withdrawal failed.");
        } else {
          toast.error("Withdrawal failed.");
        }
      } finally {
        setIsMutating(false);
      }
    },
    [activeWallet, refetch, queryClient]
  );

  const transfer = useCallback(
    async (targetWalletId: string, amount: number, description?: string) => {
      if (!activeWallet) {
        toast.error("No active wallet selected.");
        return;
      }
      setIsMutating(true);
      try {
        const res = await walletApi.transfer(
          activeWallet.id,
          {
            targetWalletId,
            amount,
            currency: activeWallet.currency,
            reference: `TRF-${Date.now().toString().slice(-6)}`,
            description: description || "P2P Wallet Transfer",
          },
          uuidv4()
        );
        toast.success(`Transferred ${res.currency} ${amount.toLocaleString()} to ${targetWalletId.slice(0, 8)}...!`);
        await refetch();
        queryClient.invalidateQueries({ queryKey: ["wallets", "transactions", activeWallet.id] });
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          toast.error(err.response?.data?.detail || "Transfer failed.");
        } else {
          toast.error("Transfer failed.");
        }
      } finally {
        setIsMutating(false);
      }
    },
    [activeWallet, refetch, queryClient]
  );

  const toggleLock = useCallback(
    async (reason?: string) => {
      if (!activeWallet) return;
      setIsMutating(true);
      try {
        if (String(activeWallet.status) === "1" || String(activeWallet.status).toLowerCase() === "active") {
          await walletApi.lock(activeWallet.id, reason || "User security freeze");
          toast.warning("Wallet locked successfully.");
        } else {
          await walletApi.unlock(activeWallet.id, reason || "User unfreeze");
          toast.success("Wallet unlocked successfully.");
        }
        await refetch();
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          toast.error(err.response?.data?.detail || "Status change failed.");
        } else {
          toast.error("Status change failed.");
        }
      } finally {
        setIsMutating(false);
      }
    },
    [activeWallet, refetch]
  );

  const refresh = useCallback(async () => {
    await refetch();
  }, [refetch]);

  return (
    <WalletContext.Provider
      value={{
        wallets: userWallets,
        activeWallet,
        setActiveWalletId,
        isLoading,
        isMutating,
        createWallet,
        deposit,
        withdraw,
        transfer,
        toggleLock,
        refresh,
      }}
    >
      {children}
    </WalletContext.Provider>
  );
}

export function useWallet() {
  const context = useContext(WalletContext);
  if (!context) {
    throw new Error("useWallet must be used within a WalletProvider");
  }
  return context;
}
