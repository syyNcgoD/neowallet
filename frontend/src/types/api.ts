export interface WalletSummaryDto {
  id: string;
  ownerId: string;
  currency: string;
  balance: number;
  status: "Active" | "Locked";
  version: number;
  createdAtUtc: string;
}

export type TransactionType = "Deposit" | "Withdraw" | "TransferIn" | "TransferOut";

export interface TransactionHistoryDto {
  id: string;
  walletId: string;
  amount: number;
  currency: string;
  type: TransactionType;
  balanceAfter: number;
  reference?: string | null;
  description?: string | null;
  relatedWalletId?: string | null;
  occurredAtUtc: string;
}

export interface CreateWalletRequest {
  ownerId: string;
  currency: string;
}

export interface WalletCreatedResponse {
  id: string;
  ownerId: string;
  currency: string;
  balance: number;
  status: string;
}

export interface DepositRequest {
  amount: number;
  currency: string;
  reference?: string;
  description?: string;
}

export interface WithdrawRequest {
  amount: number;
  currency: string;
  reference?: string;
  description?: string;
}

export interface TransferRequest {
  targetWalletId: string;
  amount: number;
  currency: string;
  reference?: string;
  description?: string;
}

export interface LockWalletRequest {
  reason: string;
}

export interface UnlockWalletRequest {
  reason: string;
}

export interface PaymentInitiateRequest {
  walletId: string;
  amount: number;
  currency: string;
  callbackUrl?: string;
}

export interface PaymentInitiatedResponse {
  paymentId: string;
  gatewayToken: string;
  checkoutUrl: string;
  amount: number;
  currency: string;
}

export interface PaymentVerifyRequest {
  paymentId: string;
  gatewayToken: string;
}

export interface PaymentVerificationResponse {
  paymentId: string;
  status: string;
  amount: number;
  currency: string;
  trackingNumber: string;
}

export interface AuditVerificationResultDto {
  isValid: boolean;
  totalEntriesChecked: number;
  lastVerifiedHash: string;
  compromisedAtSequence?: number | null;
  message: string;
}

export interface DiscrepancyDto {
  walletId: string;
  ledgerBalance: number;
  eventReplayBalance: number;
  difference: number;
  explanation: string;
}

export interface ReconciliationReportDto {
  id: string;
  generatedAtUtc: string;
  totalWalletsChecked: number;
  totalTransactionsChecked: number;
  totalDiscrepanciesFound: number;
  isBalanced: boolean;
  totalDiscrepancyAmount: number;
  discrepancies: DiscrepancyDto[];
}

export interface ApiKeyDto {
  id: string;
  prefix: string;
  name: string;
  permissions: string[];
  createdAtUtc: string;
}

export interface CreateApiKeyRequest {
  name: string;
  permissions: string[];
}

export interface CreatedApiKeyResponse {
  id: string;
  apiKey: string;
  prefix: string;
  name: string;
  permissions: string[];
}
