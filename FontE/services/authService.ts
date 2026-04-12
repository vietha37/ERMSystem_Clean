import api from './api';
import { AuthResponse, UserRole } from "./types";

const TOKEN_KEY = 'emr_auth_token';
const ROLE_CLAIM_URI = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const NAME_CLAIM_URI = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

type JwtPayload = {
  role?: UserRole;
  unique_name?: string;
  name?: string;
  sub?: string;
  [ROLE_CLAIM_URI]?: UserRole;
  [NAME_CLAIM_URI]?: string;
  exp?: number;
};

function parseJwtPayload(token: string): JwtPayload | null {
  const parts = token.split(".");
  if (parts.length !== 3) {
    return null;
  }

  try {
    const encoded = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const decoded = atob(encoded);
    return JSON.parse(decoded) as JwtPayload;
  } catch {
    return null;
  }
}

export const authService = {
  login: async (username: string, password: string): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/login', { username, password });
    const token = response.data?.token;
    if (!token) {
      throw new Error('Authentication failed: no token received from server.');
    }
    localStorage.setItem(TOKEN_KEY, token);
    return response.data;
  },

  logout: () => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(TOKEN_KEY);
    }
  },

  getToken: (): string | null => {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(TOKEN_KEY);
    }
    return null;
  },

  isAuthenticated: (): boolean => {
    return !!authService.getToken();
  },

  getRole: (): UserRole | null => {
    const token = authService.getToken();
    if (!token) {
      return null;
    }

    const payload = parseJwtPayload(token);
    if (!payload) {
      return null;
    }

    return payload[ROLE_CLAIM_URI] ?? payload.role ?? null;
  },

  getUsername: (): string | null => {
    const token = authService.getToken();
    if (!token) {
      return null;
    }

    const payload = parseJwtPayload(token);
    if (!payload) {
      return null;
    }

    return payload[NAME_CLAIM_URI] ?? payload.unique_name ?? payload.name ?? null;
  },

  isTokenExpired: (): boolean => {
    const token = authService.getToken();
    if (!token) {
      return true;
    }

    const payload = parseJwtPayload(token);
    if (!payload?.exp) {
      return false;
    }

    return Date.now() >= payload.exp * 1000;
  },
};
