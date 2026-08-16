import { useMutation } from "@tanstack/react-query";
import { paymentApi } from "@/lib/api/payments";
import type { PaymentInitiateRequest, PaymentVerifyRequest } from "@/types/api";
import { toast } from "sonner";

export function useInitiatePayment() {
  return useMutation({
    mutationFn: (data: PaymentInitiateRequest) => paymentApi.initiate(data),
    onSuccess: (res) => {
      toast.success(`Payment initiated (${res.currency} ${res.amount})`);
    },
    onError: () => {
      toast.error("Failed to initiate payment gateway session.");
    },
  });
}

export function useVerifyPayment() {
  return useMutation({
    mutationFn: (data: PaymentVerifyRequest) => paymentApi.verify(data),
    onSuccess: (res) => {
      toast.success(`Payment verified! Tracking No: ${res.trackingNumber}`);
    },
    onError: () => {
      toast.error("Payment verification failed.");
    },
  });
}
