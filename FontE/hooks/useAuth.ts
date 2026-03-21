"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { authService } from '@/services/authService';
import toast from 'react-hot-toast';

export function useAuth() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    // Check initial auth status on mount
    const checkAuth = () => {
      const authStatus = authService.isAuthenticated();
      setIsAuthenticated(authStatus);
      setIsLoading(false);
    };
    checkAuth();
  }, []);

  const login = async (username: string, password: string, remember: boolean = false) => {
    try {
      await authService.login(username, password);
      setIsAuthenticated(true);
      toast.success('Login successful!');
      
      // Additional remember me logic if needed
      if (remember) {
         localStorage.setItem('emr_remember_me', 'true');
      } else {
         localStorage.removeItem('emr_remember_me');
      }

      router.push('/dashboard');
      return { success: true };
    } catch (error: any) {
      const msg = error?.response?.data?.message || 'Invalid email or password';
      toast.error(msg);
      return { success: false, error: msg };
    }
  };

  const logout = () => {
    authService.logout();
    setIsAuthenticated(false);
    toast.success('Logged out successfully');
    router.push('/login');
  };

  return {
    isAuthenticated,
    isLoading,
    login,
    logout,
  };
}
