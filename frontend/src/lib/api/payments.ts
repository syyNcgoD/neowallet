import { apiClient } from "./client";
import type {
  PaymentInitiateRequest,
  PaymentInitiatedResponse,
  PaymentVerifyRequest,
  PaymentVerificationResponse,
} from "@/types/api";

export const paymentApi = {
  initiate: async (data: PaymentInitiateRequest): Promise<PaymentInitiatedResponse> => {
    const res = await apiClient.post<PaymentInitiatedResponse>("/payments/initiate", data);
    return res.data;
  },

  verify: async (data: PaymentVerifyRequest): Promise<PaymentVerificationResponse> => {
    const res = await apiClient.post<PaymentVerificationResponse>("/payments/verify", data);
    return res.data;
  },
};
