"use client";

import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import ProtectedLayout from "@/components/layout/ProtectedLayout";
import { useAuth } from "@/hooks/useAuth";
import { authService } from "@/services/authService";
import { getApiErrorMessage } from "@/services/error";
import { hospitalPatientPortalService } from "@/services/hospitalPatientPortalService";
import {
  HospitalPatientPortalAppointment,
  HospitalPatientPortalOverview,
} from "@/services/types";

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
      return "Đã xếp lịch";
    case "Completed":
      return "Đã hoàn thành";
    case "Cancelled":
      return "Đã hủy";
    case "Pending":
      return "Đang chờ";
    default:
      return status;
  }
}

function getStatusStyle(status: string): string {
  switch (status) {
    case "Scheduled":
      return "border border-cyan-200 bg-cyan-50 text-cyan-700";
    case "Completed":
      return "border border-emerald-200 bg-emerald-50 text-emerald-700";
    case "Cancelled":
      return "border border-rose-200 bg-rose-50 text-rose-700";
    default:
      return "border border-slate-200 bg-slate-100 text-slate-700";
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
        toast.error(getApiErrorMessage(error, "Không thể tải cổng thông tin bệnh nhân."));
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
                  Cổng thông tin bệnh nhân
                </p>
                <h1 className="mt-4 text-4xl font-bold tracking-tight text-slate-950 md:text-5xl">
                  Xin chào {profile?.fullName ?? authService.getUsername() ?? "bạn"}.
                </h1>
                <p className="mt-4 max-w-2xl text-base leading-8 text-slate-600">
                  Theo dõi thông tin hồ sơ và lịch hẹn khám tại bệnh viện tư ngay trên một giao diện riêng cho
                  người bệnh. Dữ liệu hiện được đọc trực tiếp từ hospital database mới.
                </p>

                <div className="mt-8 grid gap-4 md:grid-cols-3">
                  <StatCard label="Mã bệnh án" value={profile?.medicalRecordNumber ?? "--"} />
                  <StatCard label="Lịch sắp tới" value={stats.totalUpcoming.toString()} />
                  <StatCard
                    label="Lần khám tiếp theo"
                    value={stats.nextAppointment ? formatDateTime(stats.nextAppointment) : "Chưa có"}
                  />
                </div>
              </div>

              <div className="rounded-[2rem] bg-slate-950 p-6 text-white shadow-[0_20px_55px_rgba(15,23,42,0.14)]">
                <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200">
                  Trạng thái tài khoản
                </p>
                <div className="mt-5 rounded-[1.5rem] border border-white/10 bg-white/5 p-5">
                  <p className="text-sm text-slate-300">Trạng thái portal</p>
                  <p className="mt-2 text-2xl font-bold text-white">
                    {profile?.portalStatus ?? "Đang đồng bộ"}
                  </p>
                  <p className="mt-3 text-sm leading-7 text-slate-300">
                    Kích hoạt từ: {profile?.activatedAtUtc ? formatDateTime(profile.activatedAtUtc) : "--"}
                  </p>
                </div>

                <div className="mt-6 space-y-3 text-sm leading-7 text-slate-300">
                  <p>Portal này được tách riêng khỏi dashboard vận hành nội bộ.</p>
                  <p>Bước tiếp theo sẽ nối thêm đơn thuốc, kết quả xét nghiệm và thanh toán.</p>
                </div>

                <button
                  onClick={() => void logout()}
                  className="mt-8 w-full rounded-full border border-cyan-300/40 px-5 py-3 text-sm font-semibold text-cyan-100 transition hover:border-cyan-200 hover:bg-white/10"
                >
                  Đăng xuất
                </button>
              </div>
            </div>
          </section>

          <div className="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
            <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-700">
                    Hồ sơ của tôi
                  </p>
                  <h2 className="mt-2 text-2xl font-bold text-slate-900">Thông tin cơ bản</h2>
                </div>
                {isLoading && (
                  <div className="rounded-full bg-cyan-50 px-4 py-2 text-xs font-bold uppercase tracking-[0.2em] text-cyan-700">
                    Đang tải
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
                  <InfoCard label="Họ và tên" value={profile.fullName} />
                  <InfoCard label="Ngày sinh" value={formatDate(profile.dateOfBirth)} />
                  <InfoCard label="Giới tính" value={profile.gender} />
                  <InfoCard label="Số điện thoại" value={profile.phone ?? "--"} />
                  <InfoCard label="Email" value={profile.email ?? "--"} />
                  <InfoCard label="Địa chỉ" value={profile.address ?? "--"} className="md:col-span-2" />
                </div>
              ) : (
                <div className="mt-6 rounded-[1.5rem] border border-rose-200 bg-rose-50 p-5 text-sm text-rose-700">
                  Không tìm thấy hồ sơ hospital portal của tài khoản này.
                </div>
              )}
            </section>

            <section className="space-y-6">
              <AppointmentPanel
                title="Lịch hẹn sắp tới"
                description="Các lịch hẹn sẽ diễn ra trong những ngày tiếp theo."
                appointments={upcomingAppointments}
                emptyMessage="Bạn chưa có lịch hẹn sắp tới."
              />

              <AppointmentPanel
                title="Lịch sử gần đây"
                description="Tổng hợp những lần khám gần nhất của bạn."
                appointments={recentAppointments}
                emptyMessage="Chưa có lịch sử khám nào trong portal."
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
      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-cyan-700">{label}</p>
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
      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">{label}</p>
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
      <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-700">{title}</p>
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
                  <h3 className="mt-2 text-lg font-bold text-slate-900">{appointment.doctorName}</h3>
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
                <MiniInfo label="Thời gian" value={formatDateTime(appointment.appointmentStartLocal)} />
                <MiniInfo label="Kênh đặt lịch" value={appointment.bookingChannel} />
                <MiniInfo label="Loại lịch hẹn" value={appointment.appointmentType} />
                <MiniInfo label="Lý do khám" value={appointment.chiefComplaint ?? "--"} />
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
      <p className="text-[11px] font-semibold uppercase tracking-[0.2em] text-slate-400">{label}</p>
      <p className="mt-2 text-sm font-medium text-slate-700">{value}</p>
    </div>
  );
}
