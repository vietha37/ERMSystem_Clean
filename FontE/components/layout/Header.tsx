"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import { notificationService } from "@/services/notificationService";
import { AppointmentNotification } from "@/services/types";

function formatTime(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "--:--";
  return date.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit", hour12: false });
}

function computeUnreadCount(
  items: AppointmentNotification[],
  fallbackUnread: number,
  viewedAt: number
): number {
  if (viewedAt <= 0) return fallbackUnread;

  const unseenByTime = items.filter((item) => {
    const t = new Date(item.appointmentDate).getTime();
    return Number.isFinite(t) && t > viewedAt;
  }).length;

  return unseenByTime;
}

export function Header() {
  const { logout, role, username, isAuthenticated } = useAuth();
  const [notifications, setNotifications] = useState<AppointmentNotification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isOpen, setIsOpen] = useState(false);
  const [isLoadingNotifications, setIsLoadingNotifications] = useState(false);
  const [lastViewedAt, setLastViewedAt] = useState<number>(0);
  const notificationRef = useRef<HTMLDivElement>(null);

  const storageKey = useMemo(
    () => `emr_notifications_seen_at_${username ?? "anonymous"}`,
    [username]
  );

  useEffect(() => {
    if (!isAuthenticated) {
      setLastViewedAt(0);
      return;
    }

    const raw = typeof window !== "undefined" ? window.localStorage.getItem(storageKey) : null;
    const parsed = raw ? Number(raw) : 0;
    setLastViewedAt(Number.isFinite(parsed) ? parsed : 0);
  }, [isAuthenticated, storageKey]);

  useEffect(() => {
    if (!isAuthenticated) {
      setNotifications([]);
      setUnreadCount(0);
      return;
    }

    let isCancelled = false;
    const fetchNotifications = async () => {
      setIsLoadingNotifications(true);
      try {
        const data = await notificationService.getToday();
        if (!isCancelled) {
          const nextNotifications = data.notifications ?? [];
          setNotifications(nextNotifications);
          setUnreadCount(
            computeUnreadCount(nextNotifications, data.unreadCount ?? 0, lastViewedAt)
          );
        }
      } catch {
        if (!isCancelled) {
          setNotifications([]);
          setUnreadCount(0);
        }
      } finally {
        if (!isCancelled) {
          setIsLoadingNotifications(false);
        }
      }
    };

    fetchNotifications();
    const timer = setInterval(fetchNotifications, 30000);
    return () => {
      isCancelled = true;
      clearInterval(timer);
    };
  }, [isAuthenticated, role, lastViewedAt]);

  useEffect(() => {
    if (!isOpen) return;

    const onPointerDown = (event: MouseEvent) => {
      const target = event.target as Node;
      if (notificationRef.current && !notificationRef.current.contains(target)) {
        setIsOpen(false);
      }
    };

    const onEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onEscape);

    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onEscape);
    };
  }, [isOpen]);

  const handleToggleNotifications = () => {
    setIsOpen((current) => {
      const next = !current;
      if (next) {
        const now = Date.now();
        setLastViewedAt(now);
        setUnreadCount(0);
        if (typeof window !== "undefined") {
          window.localStorage.setItem(storageKey, now.toString());
        }
      }
      return next;
    });
  };

  const title = useMemo(() => {
    if (role === "Admin") return "Tat ca lich kham hom nay";
    if (role === "Doctor") return "Lich kham cua ban hom nay";
    return "Thong bao hom nay";
  }, [role]);

  return (
    <header className="relative z-20 h-20 bg-white/90 backdrop-blur-md border-b border-gray-100 flex items-center justify-between px-8 shadow-sm transition-all duration-300">
      <div>
        <h2 className="text-xl font-bold text-gray-800 tracking-tight">Trung tam dieu hanh</h2>
        <p className="text-sm text-gray-500 font-medium">
          {role ?? "Nguoi dung"}{username ? ` - ${username}` : ""}
        </p>
      </div>
      <div className="flex items-center gap-5">
        <button
          onClick={logout}
          className="text-sm font-bold text-red-500 bg-red-50 hover:bg-red-100 hover:text-red-600 px-4 py-2 rounded-xl transition-colors shadow-sm"
        >
          Dang xuat
        </button>

        <div ref={notificationRef} className="relative z-30">
          <button
            onClick={handleToggleNotifications}
            className="p-2.5 rounded-full bg-gray-50 hover:bg-gray-100 relative text-gray-500 transition-colors shadow-sm"
            aria-label="Thong bao"
          >
            <span className="text-xl">🔔</span>
            {unreadCount > 0 && (
              <span className="absolute -top-1 -right-1 min-w-5 h-5 px-1 rounded-full bg-blue-600 text-white text-[10px] font-bold flex items-center justify-center border-2 border-white">
                {unreadCount > 99 ? "99+" : unreadCount}
              </span>
            )}
          </button>

          {isOpen && (
            <div className="absolute right-0 mt-2 w-[380px] max-h-[420px] overflow-auto rounded-2xl border border-gray-200 bg-white shadow-xl z-40">
              <div className="sticky top-0 bg-white border-b border-gray-100 px-4 py-3">
                <p className="text-sm font-bold text-gray-800">{title}</p>
                <p className="text-xs text-gray-500">
                  {unreadCount} thong bao
                </p>
              </div>

              {isLoadingNotifications ? (
                <div className="p-4 text-sm text-gray-500">Dang tai thong bao...</div>
              ) : notifications.length === 0 ? (
                <div className="p-4 text-sm text-gray-500">Khong co lich kham nao hom nay.</div>
              ) : (
                <ul className="divide-y divide-gray-100">
                  {notifications.map((item) => (
                    <li key={item.appointmentId} className="px-4 py-3 hover:bg-blue-50/60 transition-colors">
                      <div className="flex items-center justify-between gap-2">
                        <p className="text-sm font-semibold text-gray-800 truncate">{item.patientName}</p>
                        <span className="text-xs font-bold text-blue-700">{formatTime(item.appointmentDate)}</span>
                      </div>
                      <p className="text-xs text-gray-600 mt-1">
                        Bac si: {item.doctorName}
                      </p>
                      <p className="text-xs text-gray-500 mt-1 truncate">{item.message}</p>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </div>

        <div className="flex items-center gap-3 cursor-pointer hover:opacity-80 transition-opacity">
          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white font-bold shadow-md ring-2 ring-blue-50">
            {(role ?? "U").charAt(0)}
          </div>
          <div className="hidden md:block text-left">
            <p className="text-sm font-bold text-gray-800 leading-tight">{role ?? "Nguoi dung"}</p>
            <p className="text-[11px] text-blue-600 font-bold uppercase tracking-wider">Dang hoat dong</p>
          </div>
        </div>
      </div>
    </header>
  );
}
