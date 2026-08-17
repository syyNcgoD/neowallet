import { apiClient } from "./client";
import type {
  WalletSummaryDto,
  WalletCreatedResponse,
  CreateWalletRequest,
  DepositRequest,
  WithdrawRequest,
  TransferRequest,
  TransactionHistoryDto,
} from "@/types/api";

export const walletApi = {
  getSummary: async (id: string): Promise<WalletSummaryDto> => {
    const res = await apiClient.get<WalletSummaryDto>(`/wallets/${id}/summary`);
    return res.data;
  },

  getUserWallets: async (ownerId: string): Promise<WalletSummaryDto[]> => {
    const res = await apiClient.get<WalletSummaryDto[]>(`/wallets/user/${ownerId}`);
    return res.data;
  },

  create: async (data: CreateWalletRequest): Promise<WalletCreatedResponse> => {
    const res = await apiClient.post<WalletCreatedResponse>("/wallets", data);
    return res.data;
  },

  deposit: async (
    id: string,
    data: DepositRequest,
    idempotencyKey?: string
  ): Promise<{ transactionId: string; newBalance: number; currency: string }> => {
    const headers = idempotencyKey ? { "Idempotency-Key": idempotencyKey } : {};
    const res = await apiClient.post(`/wallets/${id}/deposit`, data, { headers });
    return res.data;
  },

  withdraw: async (
    id: string,
    data: WithdrawRequest,
    idempotencyKey?: string
  ): Promise<{ transactionId: string; newBalance: number; currency: string }> => {
    const headers = idempotencyKey ? { "Idempotency-Key": idempotencyKey } : {};
    const res = await apiClient.post(`/wallets/${id}/withdraw`, data, { headers });
    return res.data;
  },

  transfer: async (
    id: string,
    data: TransferRequest,
    idempotencyKey?: string
  ): Promise<{ transactionId: string; sourceWalletId: string; targetWalletId: string; amount: number; currency: string }> => {
    const headers = idempotencyKey ? { "Idempotency-Key": idempotencyKey } : {};
    const res = await apiClient.post(`/wallets/${id}/transfer`, data, { headers });
    return res.data;
  },

  lock: async (id: string, reason: string): Promise<{ walletId: string; status: string }> => {
    const res = await apiClient.post(`/wallets/${id}/lock`, { reason });
    return res.data;
  },

  unlock: async (id: string, reason: string): Promise<{ walletId: string; status: string }> => {
    const res = await apiClient.post(`/wallets/${id}/unlock`, { reason });
    return res.data;
  },

  getTransactions: async (id: string): Promise<TransactionHistoryDto[]> => {
    const res = await apiClient.get<TransactionHistoryDto[]>(`/wallets/${id}/transactions`);
    return res.data;
  },
};
