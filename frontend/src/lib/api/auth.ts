import { apiClient } from "./client";
import type {
  ApiKeyDto,
  CreateApiKeyRequest,
  CreatedApiKeyResponse,
} from "@/types/api";

export const authApi = {
  enable2FA: async (totpCode: string): Promise<{ message: string; isTwoFactorEnabled: boolean }> => {
    const res = await apiClient.post("/auth/2fa/enable", { totpCode });
    return res.data;
  },

  disable2FA: async (totpCode: string): Promise<{ message: string; isTwoFactorEnabled: boolean }> => {
    const res = await apiClient.post("/auth/2fa/disable", { totpCode });
    return res.data;
  },

  getApiKeys: async (): Promise<ApiKeyDto[]> => {
    const res = await apiClient.get<ApiKeyDto[]>("/auth/api-keys");
    return res.data;
  },

  createApiKey: async (data: CreateApiKeyRequest): Promise<CreatedApiKeyResponse> => {
    const res = await apiClient.post<CreatedApiKeyResponse>("/auth/api-keys", data);
    return res.data;
  },

  deleteApiKey: async (id: string): Promise<{ message: string }> => {
    const res = await apiClient.delete(`/auth/api-keys/${id}`);
    return res.data;
  },
};
