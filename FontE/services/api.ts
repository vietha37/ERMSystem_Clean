import axios from 'axios';

// Create a configured axios instance
const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5219/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to attach token
api.interceptors.request.use(
  (config) => {
    // Attach JWT token from localStorage
    if (typeof window !== "undefined") {
      const token = localStorage.getItem('emr_auth_token');
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor to handle token expiration/errors
api.interceptors.response.use(
  (response) => response,
  (error) => {

    const originalRequest = error.config

    if (
      error.response?.status === 401 &&
      !originalRequest.url.includes("/api/auth/login")
    ) {
      localStorage.removeItem("emr_auth_token")
      window.location.href = "/login"
    }

    return Promise.reject(error)
  }
)

export default api;
