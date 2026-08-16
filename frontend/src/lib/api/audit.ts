import { apiClient } from "./client";
import type {
  AuditVerificationResultDto,
  ReconciliationReportDto,
} from "@/types/api";

export const auditApi = {
  verifyChain: async (): Promise<AuditVerificationResultDto> => {
    const res = await apiClient.get<AuditVerificationResultDto>("/audit/verify");
    return res.data;
  },

  reconcile: async (): Promise<ReconciliationReportDto> => {
    const res = await apiClient.post<ReconciliationReportDto>("/audit/reconcile");
    return res.data;
  },

  getLatestReconciliation: async (): Promise<ReconciliationReportDto> => {
    const res = await apiClient.get<ReconciliationReportDto>("/audit/reconcile/latest");
    return res.data;
  },
};
