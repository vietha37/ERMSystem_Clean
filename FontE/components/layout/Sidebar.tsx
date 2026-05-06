"use client";

import Link from "next/link";
import { useAuth } from "@/hooks/useAuth";
import { UserRole } from "@/services/types";

export function Sidebar() {
  const { logout, role } = useAuth();

  const menuItemsByRole: Record<UserRole, Array<{ name: string; path: string }>> = {
    Admin: [
      { name: "Tong quan", path: "/dashboard" },
      { name: "Worklist bac si", path: "/doctor-worklist" },
      { name: "Nhan su", path: "/staff" },
      { name: "Benh nhan", path: "/patients" },
      { name: "Lich hen", path: "/appointments" },
      { name: "Ho so benh an", path: "/medical-records" },
      { name: "Don thuoc", path: "/prescriptions" },
      { name: "Thong bao", path: "/notifications" },
    ],
    Doctor: [
      { name: "Tong quan", path: "/dashboard" },
      { name: "Worklist bac si", path: "/doctor-worklist" },
      { name: "Benh nhan", path: "/patients" },
      { name: "Lich hen", path: "/appointments" },
      { name: "Ho so benh an", path: "/medical-records" },
      { name: "Don thuoc", path: "/prescriptions" },
    ],
    Receptionist: [
      { name: "Tong quan", path: "/dashboard" },
      { name: "Worklist bac si", path: "/doctor-worklist" },
      { name: "Benh nhan", path: "/patients" },
      { name: "Lich hen", path: "/appointments" },
      { name: "Thong bao", path: "/notifications" },
    ],
    Patient: [
      { name: "Cong thong tin benh nhan", path: "/portal" },
    ],
  };

  const menuItems = role ? menuItemsByRole[role] : [];

  return (
    <aside className="fixed left-0 top-0 z-20 flex h-screen w-64 flex-col border-r border-gray-200 bg-white shadow-sm">
      <div className="border-b border-gray-100 p-6">
        <h1 className="text-2xl font-bold text-blue-600">ERM Hospital</h1>
        <p className="mt-1 text-xs font-medium uppercase tracking-wide text-gray-400">
          {role ?? "Khong xac dinh"}
        </p>
      </div>

      <nav className="flex-1 space-y-2 overflow-y-auto p-4">
        {menuItems.map((item) => (
          <Link
            key={item.path}
            href={item.path}
            className="flex items-center rounded-xl px-4 py-3 text-gray-700 transition-colors hover:bg-blue-50 hover:text-blue-600"
          >
            <span className="font-medium">{item.name}</span>
          </Link>
        ))}
      </nav>

      <div className="border-t border-gray-100 p-4">
        <button
          onClick={logout}
          className="w-full rounded-xl px-4 py-3 text-left font-medium text-red-500 transition-colors hover:bg-red-50"
        >
          Dang xuat
        </button>
      </div>
    </aside>
  );
}
