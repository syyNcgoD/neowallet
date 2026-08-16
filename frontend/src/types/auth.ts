export interface UserDto {
  id: string;
  email: string;
  role: number;
  isTwoFactorEnabled: boolean;
  createdAtUtc: string;
  lastLoginAtUtc?: string | null;
}

export interface AuthResultDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errorCode?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
