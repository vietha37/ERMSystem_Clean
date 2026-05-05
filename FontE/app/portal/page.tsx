"use client";

import { useEffect, useState } from "react";
import ProtectedLayout from "@/components/layout/ProtectedLayout";
import { useAuth } from "@/hooks/useAuth";
import { patientService } from "@/services/patientService";
import { Patient } from "@/services/types";
import { authService } from "@/services/authService";

export default function PatientPortalPage() {
  const { logout } = useAuth();
  const [profile, setProfile] = useState<Patient | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadProfile = async () => {
      try {
        const data = await patientService.getMe();
        setProfile(data);
      } finally {
        setLoading(false);
      }
    };

    void loadProfile();
  }, []);

  return (
    <ProtectedLayout>
      <div className="min-h-screen bg-[radial-gradient(circle_at_top_left,_rgba(34,211,238,0.18),_transparent_26%),linear-gradient(180deg,_#eff8ff_0%,_#ffffff_100%)] px-4 py-8 md:px-6">
        <div className="mx-auto max-w-6xl">
          <div className="rounded-[2.5rem] border border-slate-200 bg-white/90 p-8 shadow-[0_30px_90px_rgba(15,23,42,0.08)] backdrop-blur md:p-10">
            <div className="flex flex-col gap-6 md:flex-row md:items-end md:justify-between">
              <div>
                <p className="text-sm font-semibold uppercase tracking-[0.28em] text-cyan-700">Patient Portal</p>
                <h1 className="mt-4 font-serif text-5xl leading-none tracking-tight text-slate-950">
                  Xin chào {profile?.fullName ?? authService.getUsername() ?? "bạn"}.
                </h1>
                <p className="mt-4 max-w-2xl text-base leading-8 text-slate-600">
                  Đây là cổng dành riêng cho bệnh nhân để theo dõi hồ sơ cơ bản, lịch hẹn và các dịch vụ chăm sóc
                  tiếp theo. Lớp portal này tách biệt hoàn toàn với dashboard vận hành nội bộ.
                </p>
              </div>
              <div className="flex flex-col items-start gap-3 md:items-end">
                <div className="rounded-[1.6rem] bg-slate-950 px-5 py-4 text-sm text-slate-100">
                  Trạng thái tài khoản: <span className="font-semibold text-cyan-200">Đã xác thực</span>
                </div>
                <button
                  onClick={() => void logout()}
                  className="rounded-full border border-slate-300 bg-white px-5 py-3 text-sm font-semibold text-slate-700 transition hover:border-cyan-600 hover:text-cyan-700"
                >
                  Đăng xuất
                </button>
              </div>
            </div>

            <div className="mt-10 grid gap-5 lg:grid-cols-[1.1fr_0.9fr]">
              <section className="rounded-[2rem] border border-slate-200 bg-slate-50 p-6">
                <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-700">Hồ sơ của tôi</p>
                {loading ? (
                  <p className="mt-6 text-sm text-slate-500">Đang tải hồ sơ bệnh nhân...</p>
                ) : profile ? (
                  <div className="mt-6 grid gap-4 md:grid-cols-2">
                    <InfoCard label="Họ và tên" value={profile.fullName} />
                    <InfoCard label="Ngày sinh" value={new Date(profile.dateOfBirth).toLocaleDateString("vi-VN")} />
                    <InfoCard label="Giới tính" value={profile.gender} />
                    <InfoCard label="Điện thoại" value={profile.phone} />
                    <InfoCard label="Địa chỉ" value={profile.address} className="md:col-span-2" />
                  </div>
                ) : (
                  <p className="mt-6 text-sm text-red-500">Không tìm thấy hồ sơ bệnh nhân gắn với tài khoản này.</p>
                )}
              </section>

              <section className="grid gap-5">
                <div className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
                  <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-700">Tiếp theo</p>
                  <ul className="mt-5 space-y-3 text-sm leading-7 text-slate-600">
                    <li>Tra cứu lịch hẹn sắp tới và yêu cầu đổi lịch trực tuyến.</li>
                    <li>Xem kết quả xét nghiệm, đơn thuốc và nhắc lịch tái khám.</li>
                    <li>Nhận thông báo chăm sóc sau khám từ đội điều phối.</li>
                  </ul>
                </div>

                <div className="rounded-[2rem] border border-slate-200 bg-slate-950 p-6 text-white shadow-[0_20px_55px_rgba(15,23,42,0.12)]">
                  <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-200">Giai đoạn kế tiếp</p>
                  <p className="mt-4 text-sm leading-7 text-slate-200">
                    Portal hiện đã có auth riêng cho bệnh nhân. Bước tiếp theo là nối lịch hẹn, kết quả xét nghiệm,
                    hồ sơ bệnh án và thanh toán dành riêng cho người bệnh.
                  </p>
                </div>
              </section>
            </div>
          </div>
        </div>
      </div>
    </ProtectedLayout>
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
    <div className={`rounded-[1.4rem] border border-white bg-white px-4 py-4 shadow-sm ${className}`.trim()}>
      <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">{label}</p>
      <p className="mt-2 text-sm font-medium leading-7 text-slate-800">{value}</p>
    </div>
  );
}
