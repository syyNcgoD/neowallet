import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { authApi } from "@/lib/api/auth";
import type { CreateApiKeyRequest } from "@/types/api";
import { toast } from "sonner";

export const API_KEY_KEYS = {
  all: ["api-keys"] as const,
};

export function useApiKeys() {
  return useQuery({
    queryKey: API_KEY_KEYS.all,
    queryFn: () => authApi.getApiKeys(),
  });
}

export function useCreateApiKey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateApiKeyRequest) => authApi.createApiKey(data),
    onSuccess: () => {
      toast.success("API key created successfully!");
      queryClient.invalidateQueries({ queryKey: API_KEY_KEYS.all });
    },
    onError: () => {
      toast.error("Failed to generate API key.");
    },
  });
}

export function useDeleteApiKey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => authApi.deleteApiKey(id),
    onSuccess: () => {
      toast.success("API key revoked successfully.");
      queryClient.invalidateQueries({ queryKey: API_KEY_KEYS.all });
    },
    onError: () => {
      toast.error("Failed to revoke API key.");
    },
  });
}
