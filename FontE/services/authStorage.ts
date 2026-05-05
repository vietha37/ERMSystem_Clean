import { AuthResponse } from "./types";

const ACCESS_TOKEN_KEY = "emr_auth_token";
const REFRESH_TOKEN_KEY = "emr_refresh_token";

function canUseStorage() {
  return typeof window !== "undefined";
}

export function getAccessToken(): string | null {
  if (!canUseStorage()) {
    return null;
  }

  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  if (!canUseStorage()) {
    return null;
  }

  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function setAuthSession(auth: AuthResponse) {
  if (!canUseStorage()) {
    return;
  }

  const accessToken = auth.accessToken || auth.token;
  if (!accessToken || !auth.refreshToken) {
    throw new Error("Authentication response is missing token data.");
  }

  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, auth.refreshToken);
}

export function clearAuthSession() {
  if (!canUseStorage()) {
    return;
  }

  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

