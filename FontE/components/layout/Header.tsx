"use client";

import React from 'react';
import { useAuth } from '@/hooks/useAuth';

export function Header() {
  const { logout } = useAuth();
  
  return (
    <header className="h-20 bg-white/90 backdrop-blur-md border-b border-gray-100 flex items-center justify-between px-8 shadow-sm transition-all duration-300">
      <div>
        <h2 className="text-xl font-bold text-gray-800 tracking-tight">Welcome back, Dr. Smith</h2>
        <p className="text-sm text-gray-500 font-medium">Have a great day at work!</p>
      </div>
      <div className="flex items-center gap-5">
        <button 
          onClick={logout}
          className="text-sm font-bold text-red-500 bg-red-50 hover:bg-red-100 hover:text-red-600 px-4 py-2 rounded-xl transition-colors shadow-sm"
        >
          Logout 🚪
        </button>
        <button className="p-2.5 rounded-full bg-gray-50 hover:bg-gray-100 relative text-gray-500 transition-colors shadow-sm">
          <span className="text-xl">🔔</span>
          <span className="absolute top-1 right-1 w-2.5 h-2.5 bg-blue-500 rounded-full border-2 border-white animate-pulse"></span>
        </button>
        <div className="flex items-center gap-3 cursor-pointer hover:opacity-80 transition-opacity">
          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white font-bold shadow-md ring-2 ring-blue-50">
            DS
          </div>
          <div className="hidden md:block text-left">
            <p className="text-sm font-bold text-gray-800 leading-tight">Dr. Smith</p>
            <p className="text-[11px] text-blue-600 font-bold uppercase tracking-wider">Cardiologist</p>
          </div>
        </div>
      </div>
    </header>
  );
}
