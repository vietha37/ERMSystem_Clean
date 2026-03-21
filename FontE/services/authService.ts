import api from './api';

const TOKEN_KEY = 'emr_auth_token';

export const authService = {
  login: async (username: string, password: string) => {
    const response = await api.post('/auth/login', { username, password });
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

  getToken: () => {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(TOKEN_KEY);
    }
    return null;
  },

  isAuthenticated: () => {
    return !!authService.getToken();
  }
};
