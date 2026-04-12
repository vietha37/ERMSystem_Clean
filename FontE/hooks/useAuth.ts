"use client";

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { authService } from '@/services/authService';
import { getApiErrorMessage } from '@/services/error';
import { UserRole } from '@/services/types';
import toast from 'react-hot-toast';

export function useAuth() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [role, setRole] = useState<UserRole | null>(null);
  const [username, setUsername] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    // Check initial auth status on mount
    const checkAuth = () => {
      const authStatus = authService.isAuthenticated() && !authService.isTokenExpired();
      setIsAuthenticated(authStatus);
      setRole(authStatus ? authService.getRole() : null);
      setUsername(authStatus ? authService.getUsername() : null);
      setIsLoading(false);
    };
    checkAuth();
  }, []);

  const login = async (username: string, password: string, remember: boolean = false) => {
    try {
      await authService.login(username, password);
      setIsAuthenticated(true);
      setRole(authService.getRole());
      setUsername(authService.getUsername());
      toast.success('Login successful!');
      
      // Additional remember me logic if needed
      if (remember) {
         localStorage.setItem('emr_remember_me', 'true');
      } else {
         localStorage.removeItem('emr_remember_me');
      }

      router.push('/dashboard');
      return { success: true };
    } catch (error: unknown) {
      const msg = getApiErrorMessage(error, 'Invalid username or password');
      toast.error(msg);
      return { success: false, error: msg };
    }
  };

  const logout = () => {
    authService.logout();
    setIsAuthenticated(false);
    setRole(null);
    setUsername(null);
    toast.success('Logged out successfully');
    router.push('/login');
  };

  return {
    isAuthenticated,
    isLoading,
    role,
    username,
    login,
    logout,
  };
}
