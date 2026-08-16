import axios from "axios";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";
import type { AuthResultDto, UserDto } from "@/types/auth";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api";

const ACCESS_TOKEN_KEY = "neowallet_access_token";
const REFRESH_TOKEN_KEY = "neowallet_refresh_token";
const USER_KEY = "neowallet_user";

export const authService = {
  getAccessToken(): string | undefined {
    return Cookies.get(ACCESS_TOKEN_KEY);
  },

  getRefreshToken(): string | undefined {
    return Cookies.get(REFRESH_TOKEN_KEY);
  },

  getUser(): UserDto | null {
    const userJson = Cookies.get(USER_KEY);
    if (!userJson) return null;
    try {
      return JSON.parse(userJson) as UserDto;
    } catch {
      return null;
    }
  },

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;
    try {
      const decoded = jwtDecode<{ exp?: number }>(token);
      if (!decoded.exp) return true;
      return decoded.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  },

  setSession(authResult: AuthResultDto) {
    const { accessToken, refreshToken, user } = authResult;
    // Access token valid for 15 minutes, refresh token for 7 days
    Cookies.set(ACCESS_TOKEN_KEY, accessToken, { expires: 1 / 96, sameSite: "lax", secure: process.env.NODE_ENV === "production" });
    Cookies.set(REFRESH_TOKEN_KEY, refreshToken, { expires: 7, sameSite: "lax", secure: process.env.NODE_ENV === "production" });
    Cookies.set(USER_KEY, JSON.stringify(user), { expires: 7, sameSite: "lax", secure: process.env.NODE_ENV === "production" });
  },

  clearSession() {
    Cookies.remove(ACCESS_TOKEN_KEY);
    Cookies.remove(REFRESH_TOKEN_KEY);
    Cookies.remove(USER_KEY);
  },

  async login(email: string, password: string): Promise<AuthResultDto> {
    const response = await axios.post<AuthResultDto>(`${API_URL}/auth/login`, {
      email,
      password,
    });
    this.setSession(response.data);
    return response.data;
  },

  async register(email: string, password: string, role = 1): Promise<AuthResultDto> {
    const response = await axios.post<AuthResultDto>(`${API_URL}/auth/register`, {
      email,
      password,
      role,
    });
    this.setSession(response.data);
    return response.data;
  },

  async refreshToken(): Promise<string | null> {
    const accessToken = this.getAccessToken();
    const refreshToken = this.getRefreshToken();
    if (!accessToken || !refreshToken) return null;

    try {
      const response = await axios.post<AuthResultDto>(`${API_URL}/auth/refresh-token`, {
        accessToken,
        refreshToken,
      });
      this.setSession(response.data);
      return response.data.accessToken;
    } catch {
      this.clearSession();
      return null;
    }
  },

  async logout(): Promise<void> {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      try {
        await axios.post(`${API_URL}/auth/revoke-token`, { refreshToken });
      } catch {
        // Continue clearing session even if revocation call fails
      }
    }
    this.clearSession();
  },
};
