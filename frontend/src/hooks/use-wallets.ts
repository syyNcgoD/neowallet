import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { walletApi } from "@/lib/api/wallets";
import type {
  CreateWalletRequest,
  DepositRequest,
  WithdrawRequest,
  TransferRequest,
} from "@/types/api";
import { toast } from "sonner";
import axios from "axios";

export const WALLET_KEYS = {
  all: ["wallets"] as const,
  summary: (id: string) => ["wallets", "summary", id] as const,
  transactions: (id: string) => ["wallets", "transactions", id] as const,
};

export function useWalletSummary(walletId?: string) {
  return useQuery({
    queryKey: walletId ? WALLET_KEYS.summary(walletId) : ["wallets", "none"],
    queryFn: () => (walletId ? walletApi.getSummary(walletId) : Promise.reject("No wallet ID")),
    enabled: Boolean(walletId),
  });
}

export function useWalletTransactions(walletId?: string) {
  return useQuery({
    queryKey: walletId ? WALLET_KEYS.transactions(walletId) : ["wallets", "transactions", "none"],
    queryFn: () => (walletId ? walletApi.getTransactions(walletId) : Promise.reject("No wallet ID")),
    enabled: Boolean(walletId),
  });
}

export function useCreateWallet() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateWalletRequest) => walletApi.create(data),
    onSuccess: (data) => {
      toast.success(`Wallet created successfully (${data.currency})!`);
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.all });
    },
    onError: (err: unknown) => {
      if (axios.isAxiosError(err)) {
        toast.error(err.response?.data?.detail || "Failed to create wallet.");
      } else {
        toast.error("Failed to create wallet.");
      }
    },
  });
}

export function useDeposit(walletId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ data, idempotencyKey }: { data: DepositRequest; idempotencyKey?: string }) =>
      walletApi.deposit(walletId, data, idempotencyKey),
    onSuccess: (result) => {
      toast.success(`Successfully deposited ${result.currency} ${result.newBalance.toLocaleString()}`);
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(walletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.transactions(walletId) });
    },
    onError: (err: unknown) => {
      if (axios.isAxiosError(err)) {
        toast.error(err.response?.data?.detail || "Deposit failed.");
      } else {
        toast.error("Deposit failed.");
      }
    },
  });
}

export function useWithdraw(walletId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ data, idempotencyKey }: { data: WithdrawRequest; idempotencyKey?: string }) =>
      walletApi.withdraw(walletId, data, idempotencyKey),
    onSuccess: (result) => {
      toast.success(`Successfully withdrew ${result.currency}. New balance: ${result.newBalance.toLocaleString()}`);
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(walletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.transactions(walletId) });
    },
    onError: (err: unknown) => {
      if (axios.isAxiosError(err)) {
        toast.error(err.response?.data?.detail || "Withdrawal failed.");
      } else {
        toast.error("Withdrawal failed.");
      }
    },
  });
}

export function useTransfer(sourceWalletId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ data, idempotencyKey }: { data: TransferRequest; idempotencyKey?: string }) =>
      walletApi.transfer(sourceWalletId, data, idempotencyKey),
    onSuccess: (result) => {
      toast.success(`Transfer of ${result.currency} ${result.amount} completed successfully!`);
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(sourceWalletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.transactions(sourceWalletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(result.targetWalletId) });
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.transactions(result.targetWalletId) });
    },
    onError: (err: unknown) => {
      if (axios.isAxiosError(err)) {
        toast.error(err.response?.data?.detail || "Transfer failed.");
      } else {
        toast.error("Transfer failed.");
      }
    },
  });
}

export function useLockWallet(walletId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (reason: string) => walletApi.lock(walletId, reason),
    onSuccess: () => {
      toast.warning("Wallet locked successfully.");
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(walletId) });
    },
    onError: (err: unknown) => {
      if (axios.isAxiosError(err)) {
        toast.error(err.response?.data?.detail || "Failed to lock wallet.");
      } else {
        toast.error("Failed to lock wallet.");
      }
    },
  });
}

export function useUnlockWallet(walletId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (reason: string) => walletApi.unlock(walletId, reason),
    onSuccess: () => {
      toast.success("Wallet unlocked successfully.");
      queryClient.invalidateQueries({ queryKey: WALLET_KEYS.summary(walletId) });
    },
    onError: (err: unknown) => {
      if (axios.isAxiosError(err)) {
        toast.error(err.response?.data?.detail || "Failed to unlock wallet.");
      } else {
        toast.error("Failed to unlock wallet.");
      }
    },
  });
}
