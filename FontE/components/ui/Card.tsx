import React from 'react';

export function Card({ children, className = '' }: { children: React.ReactNode; className?: string }) {
  return (
    <div className={`bg-white rounded-2xl shadow-md p-4 hover:shadow-lg transition-shadow duration-300 ${className}`}>
      {children}
    </div>
  );
}
