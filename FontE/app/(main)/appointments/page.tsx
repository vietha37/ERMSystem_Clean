"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Modal } from "@/components/ui/Modal";
import { useAuth } from "@/hooks/useAuth";
import { getApiErrorMessage } from "@/services/error";
import { hospitalAppointmentWorklistService } from "@/services/hospitalAppointmentWorklistService";
import {
  HospitalAppointmentWorklistItem,
  HospitalAppointmentWorklistStatus,
} from "@/services/types";
import toast from "react-hot-toast";

const STATUS_OPTIONS: Array<{
  value: HospitalAppointmentWorklistStatus | "All";
  label: string;
}> = [
  { value: "All", label: "Tat ca" },
  { value: "Scheduled", label: "Da xep lich" },
  { value: "CheckedIn", label: "Da check-in" },
  { value: "Completed", label: "Da hoan thanh" },
  { value: "Cancelled", label: "Da huy" },
];

function getStatusLabel(status: HospitalAppointmentWorklistStatus): string {
  switch (status) {
    case "Scheduled":
      return "Da xep lich";
    case "CheckedIn":
      return "Da check-in";
    case "Completed":
      return "Da hoan thanh";
    case "Cancelled":
      return "Da huy";
    default:
      return status;
  }
}

function getStatusStyle(status: HospitalAppointmentWorklistStatus): string {
  switch (status) {
    case "Scheduled":
      return "bg-cyan-50 text-cyan-700 border border-cyan-200";
    case "CheckedIn":
      return "bg-amber-50 text-amber-700 border border-amber-200";
    case "Completed":
      return "bg-emerald-50 text-emerald-700 border border-emerald-200";
    case "Cancelled":
      return "bg-rose-50 text-rose-700 border border-rose-200";
    default:
      return "bg-slate-100 text-slate-700 border border-slate-200";
  }
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

export default function AppointmentsPage() {
  const { role } = useAuth();
  const [appointments, setAppointments] = useState<HospitalAppointmentWorklistItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<HospitalAppointmentWorklistStatus | "All">("All");
  const [appointmentDate, setAppointmentDate] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [isCheckInModalOpen, setIsCheckInModalOpen] = useState(false);
  const [selectedAppointment, setSelectedAppointment] = useState<HospitalAppointmentWorklistItem | null>(null);
  const [counterLabel, setCounterLabel] = useState("");
  const [actionId, setActionId] = useState<string | null>(null);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setDebouncedSearch(searchQuery.trim());
      setPageNumber(1);
    }, 400);

    return () => window.clearTimeout(timer);
  }, [searchQuery]);

  const fetchAppointments = useCallback(
    async (showRefreshState = false) => {
      if (showRefreshState) {
        setIsRefreshing(true);
      } else {
        setIsLoading(true);
      }

      try {
        const data = await hospitalAppointmentWorklistService.getAll({
          pageNumber,
          pageSize,
          status: statusFilter,
          appointmentDate: appointmentDate || undefined,
          textSearch: debouncedSearch || undefined,
        });

        setAppointments(data.items);
        setTotalCount(data.totalCount);
      } catch (error: unknown) {
        toast.error(getApiErrorMessage(error, "Khong the tai danh sach lich hen."));
      } finally {
        setIsLoading(false);
        setIsRefreshing(false);
      }
    },
    [appointmentDate, debouncedSearch, pageNumber, pageSize, statusFilter]
  );

  useEffect(() => {
    void fetchAppointments();
  }, [fetchAppointments]);

  const metrics = useMemo(() => {
    return appointments.reduce(
      (acc, item) => {
        acc[item.status] += 1;
        return acc;
      },
      {
        Scheduled: 0,
        CheckedIn: 0,
        Completed: 0,
        Cancelled: 0,
      } as Record<HospitalAppointmentWorklistStatus, number>
    );
  }, [appointments]);

  const openCheckInModal = (appointment: HospitalAppointmentWorklistItem) => {
    setSelectedAppointment(appointment);
    setCounterLabel(appointment.counterLabel ?? "");
    setIsCheckInModalOpen(true);
  };

  const handleCheckIn = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!selectedAppointment) {
      return;
    }

    setActionId(selectedAppointment.appointmentId);

    try {
      await hospitalAppointmentWorklistService.checkIn(selectedAppointment.appointmentId, {
        counterLabel: counterLabel.trim() || undefined,
      });

      toast.success("Da check-in benh nhan va cap so thu tu.");
      setIsCheckInModalOpen(false);
      setSelectedAppointment(null);
      setCounterLabel("");
      await fetchAppointments(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Khong the check-in lich hen."));
    } finally {
      setActionId(null);
    }
  };

  const handleStatusUpdate = async (
    appointment: HospitalAppointmentWorklistItem,
    status: HospitalAppointmentWorklistStatus
  ) => {
    setActionId(appointment.appointmentId);

    try {
      await hospitalAppointmentWorklistService.updateStatus(appointment.appointmentId, status);
      toast.success(`Da cap nhat trang thai sang ${getStatusLabel(status)}.`);
      await fetchAppointments(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Khong the cap nhat trang thai lich hen."));
    } finally {
      setActionId(null);
    }
  };

  const canCheckIn = role === "Admin" || role === "Receptionist";
  const startItem = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const endItem = totalCount === 0 ? 0 : Math.min(pageNumber * pageSize, totalCount);

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <div className="rounded-[2rem] border border-sky-100 bg-gradient-to-br from-cyan-50 via-white to-blue-50 p-6 shadow-sm">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.26em] text-cyan-700">
              Appointment service
            </p>
            <h1 className="mt-3 text-3xl font-bold text-slate-950">
              Dieu phoi lich hen noi bo
            </h1>
            <p className="mt-2 max-w-3xl text-sm leading-7 text-slate-600">
              Worklist nay doc truc tiep tu hospital database moi de le tan va bac si
              theo doi luong tiep nhan, check-in va xu ly lich hen trong ngay.
            </p>
          </div>

          <div className="flex flex-col gap-3 md:flex-row md:items-center">
            <input
              type="text"
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder="Tim theo ma lich, benh nhan, bac si..."
              className="min-w-[260px] rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
            />

            <input
              type="date"
              value={appointmentDate}
              onChange={(event) => {
                setAppointmentDate(event.target.value);
                setPageNumber(1);
              }}
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
            />

            <select
              value={statusFilter}
              onChange={(event) => {
                setStatusFilter(event.target.value as HospitalAppointmentWorklistStatus | "All");
                setPageNumber(1);
              }}
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
            >
              {STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <Button onClick={() => void fetchAppointments(true)} disabled={isRefreshing}>
              {isRefreshing ? "Dang lam moi..." : "Lam moi"}
            </Button>
          </div>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Da xep lich" value={metrics.Scheduled} tone="cyan" />
        <MetricCard label="Da check-in" value={metrics.CheckedIn} tone="amber" />
        <MetricCard label="Da hoan thanh" value={metrics.Completed} tone="emerald" />
        <MetricCard label="Da huy" value={metrics.Cancelled} tone="rose" />
      </div>

      <Card className="overflow-hidden border border-slate-100 p-0 shadow-sm">
        <div className="flex flex-col gap-3 border-b border-slate-100 px-6 py-5 md:flex-row md:items-center md:justify-between">
          <div>
            <h2 className="text-lg font-bold text-slate-900">Worklist lich hen</h2>
            <p className="mt-1 text-sm text-slate-500">
              Hien thi {startItem}-{endItem} / {totalCount} lich hen.
            </p>
          </div>

          <select
            value={pageSize}
            onChange={(event) => {
              setPageSize(Number(event.target.value));
              setPageNumber(1);
            }}
            className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
          >
            <option value={10}>10 dong / trang</option>
            <option value={20}>20 dong / trang</option>
            <option value={50}>50 dong / trang</option>
          </select>
        </div>

        {isLoading ? (
          <div className="flex flex-col items-center justify-center p-16">
            <div className="mb-4 h-10 w-10 animate-spin rounded-full border-4 border-cyan-100 border-t-cyan-600" />
            <p className="text-sm font-medium text-slate-500">Dang tai worklist lich hen...</p>
          </div>
        ) : appointments.length === 0 ? (
          <div className="p-16 text-center text-sm text-slate-500">
            Khong co lich hen nao khop bo loc hien tai.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-[1320px] w-full border-collapse text-left">
              <thead>
                <tr className="bg-slate-50">
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Lich hen</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Benh nhan</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Bac si</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Thoi gian</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Trang thai</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Check-in</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Tac vu</th>
                </tr>
              </thead>
              <tbody>
                {appointments.map((appointment) => (
                  <tr
                    key={appointment.appointmentId}
                    className="border-t border-slate-100 align-top transition-colors hover:bg-cyan-50/30"
                  >
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{appointment.appointmentNumber}</div>
                      <div className="mt-1 text-sm text-slate-500">
                        {appointment.appointmentType} / {appointment.bookingChannel}
                      </div>
                      <div className="mt-1 text-xs text-slate-400">
                        {appointment.chiefComplaint || "Khong co ly do kham"}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{appointment.patientName}</div>
                      <div className="mt-1 text-sm text-slate-500">
                        {appointment.medicalRecordNumber}
                      </div>
                      <div className="mt-1 text-sm text-slate-500">
                        {appointment.patientPhone || "--"}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{appointment.doctorName}</div>
                      <div className="mt-1 text-sm text-slate-500">
                        {appointment.specialtyName}
                      </div>
                      <div className="mt-1 text-sm text-slate-500">
                        {appointment.clinicName}
                        {(appointment.floorLabel || appointment.roomLabel) &&
                          ` / ${appointment.floorLabel ?? "--"} / ${appointment.roomLabel ?? "--"}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 text-sm text-slate-600">
                      <div>{formatDateTime(appointment.appointmentStartLocal)}</div>
                      <div className="mt-1 text-xs text-slate-400">
                        Ket thuc: {formatDateTime(appointment.appointmentEndLocal)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${getStatusStyle(appointment.status)}`}
                      >
                        {getStatusLabel(appointment.status)}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-slate-600">
                      <div>Quay: {appointment.counterLabel || "--"}</div>
                      <div className="mt-1">So thu tu: {appointment.queueNumber || "--"}</div>
                      <div className="mt-1 text-xs text-slate-400">
                        {appointment.checkInTimeLocal
                          ? `Luc ${formatDateTime(appointment.checkInTimeLocal)}`
                          : "Chua check-in"}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-wrap gap-2">
                        {canCheckIn && appointment.status === "Scheduled" && (
                          <Button
                            variant="secondary"
                            onClick={() => openCheckInModal(appointment)}
                            disabled={actionId === appointment.appointmentId}
                            className="border-amber-200 text-amber-700 hover:bg-amber-50"
                          >
                            Check-in
                          </Button>
                        )}

                        {appointment.status !== "Completed" && appointment.status !== "Cancelled" && (
                          <Button
                            variant="secondary"
                            onClick={() => void handleStatusUpdate(appointment, "Completed")}
                            disabled={actionId === appointment.appointmentId}
                            className="border-emerald-200 text-emerald-700 hover:bg-emerald-50"
                          >
                            Hoan thanh
                          </Button>
                        )}

                        {appointment.status !== "Cancelled" && (
                          <Button
                            variant="secondary"
                            onClick={() => void handleStatusUpdate(appointment, "Cancelled")}
                            disabled={actionId === appointment.appointmentId}
                            className="border-rose-200 text-rose-700 hover:bg-rose-50"
                          >
                            Huy lich
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <div className="flex flex-col gap-3 border-t border-slate-100 px-6 py-4 md:flex-row md:items-center md:justify-between">
          <p className="text-sm text-slate-500">
            Trang {pageNumber}/{totalPages}
          </p>

          <div className="flex items-center gap-2">
            <Button variant="secondary" onClick={() => setPageNumber(1)} disabled={pageNumber === 1}>
              Dau
            </Button>
            <Button
              variant="secondary"
              onClick={() => setPageNumber((current) => current - 1)}
              disabled={pageNumber === 1}
            >
              Truoc
            </Button>
            <Button
              variant="secondary"
              onClick={() => setPageNumber((current) => current + 1)}
              disabled={pageNumber >= totalPages}
            >
              Sau
            </Button>
          </div>
        </div>
      </Card>

      <Modal
        isOpen={isCheckInModalOpen}
        onClose={() => {
          setIsCheckInModalOpen(false);
          setSelectedAppointment(null);
          setCounterLabel("");
        }}
        title="Check-in benh nhan"
      >
        <form onSubmit={handleCheckIn} className="space-y-5">
          <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
            <p className="text-sm font-semibold text-slate-900">
              {selectedAppointment?.patientName}
            </p>
            <p className="mt-1 text-sm text-slate-500">
              {selectedAppointment?.appointmentNumber} / {selectedAppointment?.doctorName}
            </p>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Quay tiep nhan
            </label>
            <input
              type="text"
              value={counterLabel}
              onChange={(event) => setCounterLabel(event.target.value)}
              placeholder="Vi du: Quay 1"
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
            />
          </div>

          <div className="flex justify-end gap-3 border-t border-slate-100 pt-5">
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                setIsCheckInModalOpen(false);
                setSelectedAppointment(null);
                setCounterLabel("");
              }}
            >
              Dong
            </Button>
            <Button type="submit" disabled={!selectedAppointment || actionId === selectedAppointment.appointmentId}>
              {actionId === selectedAppointment?.appointmentId ? "Dang xu ly..." : "Xac nhan check-in"}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

function MetricCard({
  label,
  value,
  tone,
}: {
  label: string;
  value: number;
  tone: "cyan" | "amber" | "emerald" | "rose";
}) {
  const toneClasses = {
    cyan: "border-cyan-100 bg-cyan-50/70 text-cyan-700",
    amber: "border-amber-100 bg-amber-50/70 text-amber-700",
    emerald: "border-emerald-100 bg-emerald-50/70 text-emerald-700",
    rose: "border-rose-100 bg-rose-50/70 text-rose-700",
  };

  return (
    <Card className={`border p-5 shadow-sm hover:shadow-sm ${toneClasses[tone]}`}>
      <p className="text-xs font-bold uppercase tracking-[0.2em]">{label}</p>
      <p className="mt-3 text-3xl font-bold text-slate-950">{value}</p>
    </Card>
  );
}
