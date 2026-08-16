import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { auditApi } from "@/lib/api/audit";
import { toast } from "sonner";

export const AUDIT_KEYS = {
  all: ["audit"] as const,
  verify: ["audit", "verify"] as const,
  latestReconciliation: ["audit", "reconciliation", "latest"] as const,
};

export function useAuditVerification() {
  return useQuery({
    queryKey: AUDIT_KEYS.verify,
    queryFn: () => auditApi.verifyChain(),
  });
}

export function useLatestReconciliation() {
  return useQuery({
    queryKey: AUDIT_KEYS.latestReconciliation,
    queryFn: () => auditApi.getLatestReconciliation(),
  });
}

export function useTriggerReconciliation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => auditApi.reconcile(),
    onSuccess: (report) => {
      if (report.isBalanced) {
        toast.success(`Reconciliation complete: All ${report.totalWalletsChecked} wallets balanced perfectly!`);
      } else {
        toast.error(`Reconciliation alert: ${report.totalDiscrepanciesFound} discrepancy found!`);
      }
      queryClient.invalidateQueries({ queryKey: AUDIT_KEYS.all });
    },
    onError: () => {
      toast.error("Failed to execute reconciliation cycle.");
    },
  });
}
