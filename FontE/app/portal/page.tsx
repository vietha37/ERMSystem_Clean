"use client";

import { useEffect, useState } from "react";
import ProtectedLayout from "@/components/layout/ProtectedLayout";
import { useAuth } from "@/hooks/useAuth";
import { authService } from "@/services/authService";
import { getApiErrorMessage } from "@/services/error";
import { hospitalPatientPortalService } from "@/services/hospitalPatientPortalService";
import {
  HospitalPatientPortalAppointment,
  HospitalPatientPortalOverview,
} from "@/services/types";
import toast from "react-hot-toast";

function formatDate(value?: string | null): string {
  if (!value) {
    return "--";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "--";
  }

  return date.toLocaleDateString("vi-VN");
}

function formatDateTime(value?: string | null): string {
  if (!value) {
    return "--";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "--";
  }

  return date.toLocaleString("vi-VN");
}

function getStatusLabel(status: string): string {
  switch (status) {
    case "Scheduled":
      return "Da xep lich";
    case "Completed":
      return "Da hoan thanh";
    case "Cancelled":
      return "Da huy";
    case "Pending":
      return "Dang cho";
    default:
      return status;
  }
}

function getStatusStyle(status: string): string {
  switch (status) {
    case "Scheduled":
      return "bg-cyan-50 text-cyan-700 border border-cyan-200";
    case "Completed":
      return "bg-emerald-50 text-emerald-700 border border-emerald-200";
    case "Cancelled":
      return "bg-rose-50 text-rose-700 border border-rose-200";
    default:
      return "bg-slate-100 text-slate-700 border border-slate-200";
  }
}

export default function PatientPortalPage() {
  const { logout } = useAuth();
  const [overview, setOverview] = useState<HospitalPatientPortalOverview | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadOverview = async () => {
      setIsLoading(true);

      try {
        const data = await hospitalPatientPortalService.getMyOverview();
        setOverview(data);
      } catch (error: unknown) {
        toast.error(
          getApiErrorMessage(error, "Khong the tai cong thong tin benh nhan.")
        );
      } finally {
        setIsLoading(false);
      }
    };

    void loadOverview();
  }, []);

  const profile = overview?.profile;
  const upcomingAppointments = overview?.upcomingAppointments ?? [];
  const recentAppointments = overview?.recentAppointments ?? [];

  const stats = {
    totalUpcoming: upcomingAppointments.length,
    totalRecent: recentAppointments.length,
    nextAppointment: upcomingAppointments[0]?.appointmentStartLocal ?? null,
  };

  return (
    <ProtectedLayout>
      <div className="min-h-screen bg-[radial-gradient(circle_at_top_left,_rgba(6,182,212,0.16),_transparent_24%),linear-gradient(180deg,_#eff8ff_0%,_#ffffff_100%)] px-4 py-8 md:px-6">
        <div className="mx-auto max-w-7xl space-y-6">
          <section className="overflow-hidden rounded-[2.5rem] border border-cyan-100 bg-white/90 shadow-[0_30px_90px_rgba(15,23,42,0.08)] backdrop-blur">
            <div className="grid gap-8 px-8 py-8 lg:grid-cols-[1.2fr_0.8fr] lg:px-10 lg:py-10">
              <div>
                <p className="text-sm font-semibold uppercase tracking-[0.3em] text-cyan-700">
                  Cong thong tin benh nhan
                </p>
                <h1 className="mt-4 text-4xl font-bold tracking-tight text-slate-950 md:text-5xl">
                  Xin chao {profile?.fullName ?? authService.getUsername() ?? "ban"}.
                </h1>
                <p className="mt-4 max-w-2xl text-base leading-8 text-slate-600">
                  Theo doi thong tin ho so va lich hen kham tai benh vien tu ngay tren
                  mot giao dien rieng cho nguoi benh. Du lieu hien duoc doc truc tiep tu
                  hospital database moi.
                </p>

                <div className="mt-8 grid gap-4 md:grid-cols-3">
                  <StatCard label="Ma benh an" value={profile?.medicalRecordNumber ?? "--"} />
                  <StatCard label="Lich sap toi" value={stats.totalUpcoming.toString()} />
                  <StatCard
                    label="Lan kham tiep theo"
                    value={stats.nextAppointment ? formatDateTime(stats.nextAppointment) : "Chua co"}
                  />
                </div>
              </div>

              <div className="rounded-[2rem] bg-slate-950 p-6 text-white shadow-[0_20px_55px_rgba(15,23,42,0.14)]">
                <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200">
                  Trang thai tai khoan
                </p>
                <div className="mt-5 rounded-[1.5rem] border border-white/10 bg-white/5 p-5">
                  <p className="text-sm text-slate-300">Trang thai portal</p>
                  <p className="mt-2 text-2xl font-bold text-white">
                    {profile?.portalStatus ?? "Dang dong bo"}
                  </p>
                  <p className="mt-3 text-sm leading-7 text-slate-300">
                    Kich hoat tu: {profile?.activatedAtUtc ? formatDateTime(profile.activatedAtUtc) : "--"}
                  </p>
                </div>

                <div className="mt-6 space-y-3 text-sm leading-7 text-slate-300">
                  <p>Portal nay duoc tach rieng khoi dashboard van hanh noi bo.</p>
                  <p>Buoc tiep theo se noi them don thuoc, ket qua xet nghiem va thanh toan.</p>
                </div>

                <button
                  onClick={() => void logout()}
                  className="mt-8 w-full rounded-full border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 transition hover:border-cyan-200 hover:bg-white/10"
                >
                  Dang xuat
                </button>
              </div>
            </div>
          </section>

          <div className="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
            <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-700">
                    Ho so cua toi
                  </p>
                  <h2 className="mt-2 text-2xl font-bold text-slate-900">
                    Thong tin co ban
                  </h2>
                </div>
                {isLoading && (
                  <div className="rounded-full bg-cyan-50 px-4 py-2 text-xs font-bold uppercase tracking-[0.2em] text-cyan-700">
                    Dang tai
                  </div>
                )}
              </div>

              {isLoading ? (
                <div className="mt-6 grid gap-4 md:grid-cols-2">
                  {[1, 2, 3, 4, 5, 6].map((item) => (
                    <div
                      key={item}
                      className="h-24 animate-pulse rounded-[1.4rem] border border-slate-100 bg-slate-100"
                    />
                  ))}
                </div>
              ) : profile ? (
                <div className="mt-6 grid gap-4 md:grid-cols-2">
                  <InfoCard label="Ho va ten" value={profile.fullName} />
                  <InfoCard label="Ngay sinh" value={formatDate(profile.dateOfBirth)} />
                  <InfoCard label="Gioi tinh" value={profile.gender} />
                  <InfoCard label="So dien thoai" value={profile.phone ?? "--"} />
                  <InfoCard label="Email" value={profile.email ?? "--"} />
                  <InfoCard label="Dia chi" value={profile.address ?? "--"} className="md:col-span-2" />
                </div>
              ) : (
                <div className="mt-6 rounded-[1.5rem] border border-rose-200 bg-rose-50 p-5 text-sm text-rose-700">
                  Khong tim thay ho so hospital portal cua tai khoan nay.
                </div>
              )}
            </section>

            <section className="space-y-6">
              <AppointmentPanel
                title="Lich hen sap toi"
                description="Cac lich hen se dien ra trong nhung ngay tiep theo."
                appointments={upcomingAppointments}
                emptyMessage="Ban chua co lich hen sap toi."
              />

              <AppointmentPanel
                title="Lich su gan day"
                description="Tong hop nhung lan kham gan nhat cua ban."
                appointments={recentAppointments}
                emptyMessage="Chua co lich su kham nao trong portal."
              />
            </section>
          </div>
        </div>
      </div>
    </ProtectedLayout>
  );
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-[1.6rem] border border-cyan-100 bg-cyan-50/70 px-5 py-4">
      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-cyan-700">
        {label}
      </p>
      <p className="mt-3 text-lg font-bold text-slate-950">{value}</p>
    </div>
  );
}

function InfoCard({
  label,
  value,
  className = "",
}: {
  label: string;
  value: string;
  className?: string;
}) {
  return (
    <div className={`rounded-[1.4rem] border border-slate-100 bg-slate-50 px-4 py-4 ${className}`.trim()}>
      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">
        {label}
      </p>
      <p className="mt-2 text-sm font-medium leading-7 text-slate-800">{value}</p>
    </div>
  );
}

function AppointmentPanel({
  title,
  description,
  appointments,
  emptyMessage,
}: {
  title: string;
  description: string;
  appointments: HospitalPatientPortalAppointment[];
  emptyMessage: string;
}) {
  return (
    <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm">
      <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-700">
        {title}
      </p>
      <p className="mt-2 text-sm leading-7 text-slate-500">{description}</p>

      {appointments.length === 0 ? (
        <div className="mt-5 rounded-[1.4rem] border border-dashed border-slate-200 bg-slate-50 px-5 py-8 text-sm text-slate-500">
          {emptyMessage}
        </div>
      ) : (
        <div className="mt-5 space-y-4">
          {appointments.map((appointment) => (
            <article
              key={appointment.appointmentId}
              className="rounded-[1.5rem] border border-slate-100 bg-slate-50/80 p-5"
            >
              <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">
                    {appointment.appointmentNumber}
                  </p>
                  <h3 className="mt-2 text-lg font-bold text-slate-900">
                    {appointment.doctorName}
                  </h3>
                  <p className="mt-1 text-sm text-slate-600">
                    {appointment.specialtyName} / {appointment.clinicName}
                  </p>
                </div>

                <span
                  className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${getStatusStyle(appointment.status)}`}
                >
                  {getStatusLabel(appointment.status)}
                </span>
              </div>

              <div className="mt-4 grid gap-3 md:grid-cols-2">
                <MiniInfo label="Thoi gian" value={formatDateTime(appointment.appointmentStartLocal)} />
                <MiniInfo label="Kenh dat lich" value={appointment.bookingChannel} />
                <MiniInfo label="Loai lich hen" value={appointment.appointmentType} />
                <MiniInfo label="Ly do kham" value={appointment.chiefComplaint ?? "--"} />
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

function MiniInfo({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-[1.2rem] border border-white bg-white px-4 py-3 shadow-sm">
      <p className="text-[11px] font-semibold uppercase tracking-[0.2em] text-slate-400">
        {label}
      </p>
      <p className="mt-2 text-sm font-medium text-slate-700">{value}</p>
    </div>
  );
}
