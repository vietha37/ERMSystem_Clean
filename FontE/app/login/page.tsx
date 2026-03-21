"use client";

import React, { useState } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { Card } from '@/components/ui/Card';
import toast from 'react-hot-toast';

export default function LoginPage() {
  const { login } = useAuth();
  
  // Login State
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [remember, setRemember] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  // Forgot Password State
  const [isForgotPassword, setIsForgotPassword] = useState(false);
  const [resetEmail, setResetEmail] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    await login(username, password, remember);
    setIsSubmitting(false);
  };

  const handleForgotPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resetEmail) return;
    
    setIsSubmitting(true);
    // Simulate API call for password reset
    setTimeout(() => {
      toast.success('Password recovery link sent to your email!');
      setIsSubmitting(false);
      setIsForgotPassword(false);
      setResetEmail('');
    }, 1500);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-700 via-blue-600 to-cyan-500 p-4 relative overflow-hidden">
      {/* Decorative Background Elements */}
      <div className="absolute top-[-10%] left-[-10%] w-96 h-96 bg-white/10 rounded-full mix-blend-overlay filter blur-3xl animate-blob"></div>
      <div className="absolute bottom-[-10%] right-[-10%] w-96 h-96 bg-cyan-200/20 rounded-full mix-blend-overlay filter blur-3xl animate-blob animation-delay-2000"></div>

      <Card className="w-full max-w-md relative z-10 p-8 py-10 rounded-3xl shadow-2xl bg-white/95 backdrop-blur-xl border border-white/40 overflow-hidden">
        <div className="text-center mb-8 relative">
          <div className="mx-auto w-16 h-16 bg-blue-50 rounded-2xl flex items-center justify-center mb-5 text-3xl shadow-inner border border-blue-100 rotate-3 transform hover:rotate-12 transition-transform">
             🏥
          </div>
          <h1 className="text-3xl font-bold text-gray-900 tracking-tight">
             {isForgotPassword ? 'Reset Password' : 'EMR System'}
          </h1>
          <p className="text-gray-500 mt-2 text-sm font-medium">
             {isForgotPassword ? 'Enter your email to receive a recovery link.' : 'Sign in to securely access medical records.'}
          </p>
        </div>

        {/* FORGOT PASSWORD FORM */}
        {isForgotPassword ? (
          <form onSubmit={handleForgotPassword} className="space-y-5 animate-fade-in">
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1.5">Account Email</label>
              <input
                type="email"
                value={resetEmail}
                onChange={(e) => setResetEmail(e.target.value)}
                className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-4 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all placeholder:text-gray-400 font-medium text-gray-800 shadow-sm bg-gray-50/50"
                placeholder="doctor@ermapp.com"
                required
              />
            </div>

            <button 
              type="submit" 
              className="w-full py-3.5 mt-2 text-[15px] font-bold bg-blue-600 hover:bg-blue-700 text-white rounded-xl shadow-md hover:shadow-lg transition-all flex items-center justify-center gap-2 disabled:opacity-70 disabled:shadow-none"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <svg className="animate-spin h-5 w-5 text-white/80" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Sending Link...
                </>
              ) : (
                'Send Recovery Link'
              )}
            </button>
            
            <div className="text-center pt-2">
               <button 
                 type="button" 
                 onClick={() => setIsForgotPassword(false)}
                 className="text-sm font-bold text-gray-500 hover:text-gray-800 transition-colors"
               >
                 « Back to Login
               </button>
            </div>
          </form>
        ) : (
          /* LOGIN FORM */
          <form onSubmit={handleLogin} className="space-y-5 animate-fade-in">
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1.5">Username</label>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-4 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all placeholder:text-gray-400 font-medium text-gray-800 shadow-sm bg-gray-50/50"
                placeholder="Enter your username"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1.5">Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-4 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all placeholder:text-gray-400 font-medium text-gray-800 shadow-sm bg-gray-50/50"
                placeholder="••••••••"
                required
              />
            </div>

            <div className="flex items-center justify-between pt-1">
              <label className="flex items-center cursor-pointer group">
                <input 
                  type="checkbox" 
                  checked={remember}
                  onChange={(e) => setRemember(e.target.checked)}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 cursor-pointer" 
                />
                <span className="ml-2 text-sm font-bold text-gray-600 group-hover:text-blue-600 transition-colors">Remember me</span>
              </label>
              <button 
                type="button"
                onClick={() => setIsForgotPassword(true)} 
                className="text-sm font-bold text-blue-600 hover:text-blue-500 transition-colors"
              >
                Forgot password?
              </button>
            </div>

            <button 
              type="submit" 
              className="w-full py-3.5 mt-6 text-[15px] font-bold bg-blue-600 hover:bg-blue-700 text-white rounded-xl shadow-md hover:shadow-lg transition-all flex items-center justify-center gap-2 disabled:opacity-70 disabled:shadow-none"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <svg className="animate-spin h-5 w-5 text-white/80" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Authenticating...
                </>
              ) : (
                'Sign In to Dashboard'
              )}
            </button>
          </form>
        )}
      </Card>
      
      <div className="absolute bottom-6 text-center text-sm font-medium text-white/70 z-10 w-full drop-shadow-sm">
        &copy; {new Date().getFullYear()} EMR System. Secure Access Portal.
      </div>
      

    </div>
  );
}
