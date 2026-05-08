"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Modal } from "@/components/ui/Modal";
import { getApiErrorMessage } from "@/services/error";
import { hospitalEncounterService } from "@/services/hospitalEncounterService";
import {
  CreateHospitalEncounterPayload,
  HospitalEncounterDetail,
  HospitalEncounterEligibleAppointment,
  HospitalEncounterStatus,
  HospitalEncounterSummary,
  UpdateHospitalEncounterPayload,
} from "@/services/types";
import toast from "react-hot-toast";

const ENCOUNTER_STATUS_OPTIONS: Array<{
  value: HospitalEncounterStatus | "All";
  label: string;
}> = [
  { value: "All", label: "Tất cả" },
  { value: "InProgress", label: "Đang khám" },
  { value: "Finalized", label: "Đã chốt hồ sơ" },
];

type EncounterFormState = {
  appointmentId: string;
  diagnosisName: string;
  diagnosisCode: string;
  diagnosisType: string;
  encounterStatus: HospitalEncounterStatus;
  summary: string;
  subjective: string;
  objective: string;
  assessment: string;
  carePlan: string;
  heightCm: string;
  weightKg: string;
  temperatureC: string;
  pulseRate: string;
  respiratoryRate: string;
  systolicBp: string;
  diastolicBp: string;
  oxygenSaturation: string;
};

const EMPTY_FORM: EncounterFormState = {
  appointmentId: "",
  diagnosisName: "",
  diagnosisCode: "",
  diagnosisType: "Working",
  encounterStatus: "InProgress",
  summary: "",
  subjective: "",
  objective: "",
  assessment: "",
  carePlan: "",
  heightCm: "",
  weightKg: "",
  temperatureC: "",
  pulseRate: "",
  respiratoryRate: "",
  systolicBp: "",
  diastolicBp: "",
  oxygenSaturation: "",
};

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

function getEncounterStatusLabel(status: HospitalEncounterStatus): string {
  switch (status) {
    case "InProgress":
      return "Đang khám";
    case "Finalized":
      return "Đã chốt hồ sơ";
    default:
      return status;
  }
}

function getEncounterStatusClass(status: HospitalEncounterStatus): string {
  switch (status) {
    case "InProgress":
      return "border border-amber-200 bg-amber-50 text-amber-700";
    case "Finalized":
      return "border border-emerald-200 bg-emerald-50 text-emerald-700";
    default:
      return "border border-slate-200 bg-slate-100 text-slate-700";
  }
}

function parseOptionalNumber(value: string): number | null {
  if (!value.trim()) {
    return null;
  }

  const parsed = Number(value);
  return Number.isNaN(parsed) ? null : parsed;
}

function buildPayload(form: EncounterFormState): CreateHospitalEncounterPayload {
  return {
    appointmentId: form.appointmentId,
    diagnosisName: form.diagnosisName.trim(),
    diagnosisCode: form.diagnosisCode.trim() || undefined,
    diagnosisType: form.diagnosisType.trim() || "Working",
    encounterStatus: form.encounterStatus,
    summary: form.summary.trim() || undefined,
    subjective: form.subjective.trim() || undefined,
    objective: form.objective.trim() || undefined,
    assessment: form.assessment.trim() || undefined,
    carePlan: form.carePlan.trim() || undefined,
    heightCm: parseOptionalNumber(form.heightCm),
    weightKg: parseOptionalNumber(form.weightKg),
    temperatureC: parseOptionalNumber(form.temperatureC),
    pulseRate: parseOptionalNumber(form.pulseRate),
    respiratoryRate: parseOptionalNumber(form.respiratoryRate),
    systolicBp: parseOptionalNumber(form.systolicBp),
    diastolicBp: parseOptionalNumber(form.diastolicBp),
    oxygenSaturation: parseOptionalNumber(form.oxygenSaturation),
  };
}

function buildUpdatePayload(form: EncounterFormState): UpdateHospitalEncounterPayload {
  const payload = buildPayload(form);

  return {
    diagnosisName: payload.diagnosisName,
    diagnosisCode: payload.diagnosisCode,
    diagnosisType: payload.diagnosisType,
    encounterStatus: payload.encounterStatus,
    summary: payload.summary,
    subjective: payload.subjective,
    objective: payload.objective,
    assessment: payload.assessment,
    carePlan: payload.carePlan,
    heightCm: payload.heightCm,
    weightKg: payload.weightKg,
    temperatureC: payload.temperatureC,
    pulseRate: payload.pulseRate,
    respiratoryRate: payload.respiratoryRate,
    systolicBp: payload.systolicBp,
    diastolicBp: payload.diastolicBp,
    oxygenSaturation: payload.oxygenSaturation,
  };
}

function mapDetailToForm(detail: HospitalEncounterDetail): EncounterFormState {
  return {
    appointmentId: detail.appointmentId ?? "",
    diagnosisName: detail.primaryDiagnosisName ?? "",
    diagnosisCode: detail.diagnosisCode ?? "",
    diagnosisType: detail.diagnosisType ?? "Working",
    encounterStatus: detail.encounterStatus,
    summary: detail.summary ?? "",
    subjective: detail.subjective ?? "",
    objective: detail.objective ?? "",
    assessment: detail.assessment ?? "",
    carePlan: detail.carePlan ?? "",
    heightCm: detail.heightCm?.toString() ?? "",
    weightKg: detail.weightKg?.toString() ?? "",
    temperatureC: detail.temperatureC?.toString() ?? "",
    pulseRate: detail.pulseRate?.toString() ?? "",
    respiratoryRate: detail.respiratoryRate?.toString() ?? "",
    systolicBp: detail.systolicBp?.toString() ?? "",
    diastolicBp: detail.diastolicBp?.toString() ?? "",
    oxygenSaturation: detail.oxygenSaturation?.toString() ?? "",
  };
}

export default function MedicalRecordsPage() {
  const [encounters, setEncounters] = useState<HospitalEncounterSummary[]>([]);
  const [eligibleAppointments, setEligibleAppointments] = useState<
    HospitalEncounterEligibleAppointment[]
  >([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<HospitalEncounterStatus | "All">(
    "All"
  );
  const [appointmentDate, setAppointmentDate] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingEncounterId, setEditingEncounterId] = useState<string | null>(null);
  const [form, setForm] = useState<EncounterFormState>(EMPTY_FORM);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setDebouncedSearch(searchQuery.trim());
      setPageNumber(1);
    }, 400);

    return () => window.clearTimeout(timer);
  }, [searchQuery]);

  const fetchData = useCallback(
    async (showRefreshState = false) => {
      if (showRefreshState) {
        setIsRefreshing(true);
      } else {
        setIsLoading(true);
      }

      try {
        const [worklist, appointments] = await Promise.all([
          hospitalEncounterService.getAll({
            pageNumber,
            pageSize,
            encounterStatus: statusFilter,
            appointmentDate: appointmentDate || undefined,
            textSearch: debouncedSearch || undefined,
          }),
          hospitalEncounterService.getEligibleAppointments(),
        ]);

        setEncounters(worklist.items);
        setTotalCount(worklist.totalCount);
        setEligibleAppointments(appointments);
      } catch (error: unknown) {
        toast.error(getApiErrorMessage(error, "Kh�ng th? t?i d? li?u EMR."));
      } finally {
        setIsLoading(false);
        setIsRefreshing(false);
      }
    },
    [appointmentDate, debouncedSearch, pageNumber, pageSize, statusFilter]
  );

  useEffect(() => {
    void fetchData();
  }, [fetchData]);

  const metrics = useMemo(
    () =>
      encounters.reduce(
        (acc, item) => {
          acc[item.encounterStatus] += 1;
          return acc;
        },
        {
          InProgress: 0,
          Finalized: 0,
        } as Record<HospitalEncounterStatus, number>
      ),
    [encounters]
  );

  const startItem = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const endItem = totalCount === 0 ? 0 : Math.min(pageNumber * pageSize, totalCount);

  const availableAppointments = eligibleAppointments.filter(
    (item) => !item.existingEncounterId || item.existingEncounterId === editingEncounterId
  );

  const openCreateModal = () => {
    setEditingEncounterId(null);
    setForm(EMPTY_FORM);
    setIsModalOpen(true);
  };

  const openEditModal = async (encounterId: string) => {
    try {
      const detail = await hospitalEncounterService.getById(encounterId);
      setEditingEncounterId(encounterId);
      setForm(mapDetailToForm(detail));
      setIsModalOpen(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Kh�ng th? t?i chi ti?t encounter."));
    }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!editingEncounterId && !form.appointmentId) {
      toast.error("Can chon lich hen de mo encounter.");
      return;
    }

    if (!form.diagnosisName.trim()) {
      toast.error("Ch?n do�n l� tru?ng b?t bu?c.");
      return;
    }

    setIsSubmitting(true);

    try {
      if (editingEncounterId) {
        await hospitalEncounterService.update(editingEncounterId, buildUpdatePayload(form));
        toast.success("�� c?p nh?t encounter.");
      } else {
        await hospitalEncounterService.create(buildPayload(form));
        toast.success("�� t?o encounter m?i.");
      }

      setIsModalOpen(false);
      setEditingEncounterId(null);
      setForm(EMPTY_FORM);
      await fetchData(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Kh�ng th? luu h? so EMR."));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <div className="rounded-[2rem] border border-emerald-100 bg-gradient-to-br from-emerald-50 via-white to-cyan-50 p-6 shadow-sm">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.26em] text-emerald-700">
              EMR service
            </p>
            <h1 className="mt-3 text-3xl font-bold text-slate-950">
              H? so kh�m b?nh hospital
            </h1>
            <p className="mt-2 max-w-3xl text-sm leading-7 text-slate-600">
              Module nay da chuyen sang hospital database m?i. Moi ho so duoc luu
              theo mo hinh encounter, gom chan doan, ghi chu lam sang va dau hieu sinh ton.
            </p>
          </div>

          <div className="flex flex-col gap-3 md:flex-row md:items-center">
            <input
              type="text"
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder="Tim theo ma encounter, ma lich, benh nhan..."
              className="min-w-[260px] rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
            />

            <input
              type="date"
              value={appointmentDate}
              onChange={(event) => {
                setAppointmentDate(event.target.value);
                setPageNumber(1);
              }}
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
            />

            <select
              value={statusFilter}
              onChange={(event) => {
                setStatusFilter(event.target.value as HospitalEncounterStatus | "All");
                setPageNumber(1);
              }}
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
            >
              {ENCOUNTER_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <Button variant="secondary" onClick={() => void fetchData(true)} disabled={isRefreshing}>
              {isRefreshing ? "�ang l�m m?i..." : "Lam moi"}
            </Button>
            <Button onClick={openCreateModal}>Mo encounter</Button>
          </div>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <MetricCard label="�ang kh�m" value={metrics.InProgress} tone="amber" />
        <MetricCard label="�� ch?t h? so" value={metrics.Finalized} tone="emerald" />
        <MetricCard label="Lich cho mo encounter" value={availableAppointments.length} tone="cyan" />
      </div>

      <Card className="overflow-hidden border border-slate-100 p-0 shadow-sm">
        <div className="flex flex-col gap-3 border-b border-slate-100 px-6 py-5 md:flex-row md:items-center md:justify-between">
          <div>
            <h2 className="text-lg font-bold text-slate-900">Danh sach encounter</h2>
            <p className="mt-1 text-sm text-slate-500">
              Hien thi {startItem}-{endItem} / {totalCount} ho so.
            </p>
          </div>

          <select
            value={pageSize}
            onChange={(event) => {
              setPageSize(Number(event.target.value));
              setPageNumber(1);
            }}
            className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
          >
            <option value={10}>10 dong / trang</option>
            <option value={20}>20 dong / trang</option>
            <option value={50}>50 dong / trang</option>
          </select>
        </div>

        {isLoading ? (
          <div className="flex flex-col items-center justify-center p-16">
            <div className="mb-4 h-10 w-10 animate-spin rounded-full border-4 border-emerald-100 border-t-emerald-600" />
            <p className="text-sm font-medium text-slate-500">�ang t?i ho so EMR...</p>
          </div>
        ) : encounters.length === 0 ? (
          <div className="p-16 text-center text-sm text-slate-500">
            Chua co encounter nao khop bo loc hien tai.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-[1260px] w-full border-collapse text-left">
              <thead>
                <tr className="bg-slate-50">
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Encounter</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">B?nh nh�n</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Bac si</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Ch?n do�n</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Trang thai</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Cap nhat</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Tac vu</th>
                </tr>
              </thead>
              <tbody>
                {encounters.map((encounter) => (
                  <tr
                    key={encounter.encounterId}
                    className="border-t border-slate-100 align-top transition-colors hover:bg-emerald-50/30"
                  >
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{encounter.encounterNumber}</div>
                      <div className="mt-1 text-sm text-slate-500">
                        {encounter.appointmentNumber || "--"}
                      </div>
                      <div className="mt-1 text-xs text-slate-400">
                        Bat dau: {formatDateTime(encounter.startedAtLocal)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{encounter.patientName}</div>
                      <div className="mt-1 text-sm text-slate-500">
                        {encounter.medicalRecordNumber}
                      </div>
                      <div className="mt-1 text-xs text-slate-400">
                        Lich kham: {formatDateTime(encounter.appointmentStartLocal)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{encounter.doctorName}</div>
                      <div className="mt-1 text-sm text-slate-500">{encounter.specialtyName}</div>
                      <div className="mt-1 text-sm text-slate-500">{encounter.clinicName}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-emerald-700">
                        {encounter.primaryDiagnosisName || "--"}
                      </div>
                      <div className="mt-1 max-w-[280px] text-sm text-slate-500">
                        {encounter.summary || "Chua co tom tat benh an."}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${getEncounterStatusClass(
                          encounter.encounterStatus
                        )}`}
                      >
                        {getEncounterStatusLabel(encounter.encounterStatus)}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-slate-600">
                      <div>{formatDateTime(encounter.updatedAtLocal)}</div>
                      <div className="mt-1 text-xs text-slate-400">
                        Ket thuc: {formatDateTime(encounter.endedAtLocal)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <Button
                        variant="secondary"
                        className="border-emerald-200 text-emerald-700 hover:bg-emerald-50"
                        onClick={() => void openEditModal(encounter.encounterId)}
                      >
                        Cap nhat
                      </Button>
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
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false);
          setEditingEncounterId(null);
          setForm(EMPTY_FORM);
        }}
        title={editingEncounterId ? "Cap nhat encounter" : "Mo encounter moi"}
      >
        <form onSubmit={handleSubmit} className="space-y-5">
          {!editingEncounterId && (
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                L?ch h?n da check-in / da hoan thanh
              </label>
              <select
                value={form.appointmentId}
                onChange={(event) => setForm((current) => ({ ...current, appointmentId: event.target.value }))}
                className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
                required
              >
                <option value="">-- Chon lich hen --</option>
                {availableAppointments.map((appointment) => (
                  <option key={appointment.appointmentId} value={appointment.appointmentId}>
                    {appointment.appointmentNumber} - {appointment.patientName} - {formatDateTime(appointment.appointmentStartLocal)}
                  </option>
                ))}
              </select>
            </div>
          )}

          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">Ch?n do�n</label>
              <input
                type="text"
                value={form.diagnosisName}
                onChange={(event) => setForm((current) => ({ ...current, diagnosisName: event.target.value }))}
                className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
                placeholder="Vi du: Tang huyet ap"
                required
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">Ma chan doan</label>
              <input
                type="text"
                value={form.diagnosisCode}
                onChange={(event) => setForm((current) => ({ ...current, diagnosisCode: event.target.value }))}
                className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
                placeholder="ICD-10 neu co"
              />
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">Loai chan doan</label>
              <input
                type="text"
                value={form.diagnosisType}
                onChange={(event) => setForm((current) => ({ ...current, diagnosisType: event.target.value }))}
                className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
                placeholder="Working / Final"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">Trang thai encounter</label>
              <select
                value={form.encounterStatus}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    encounterStatus: event.target.value as HospitalEncounterStatus,
                  }))
                }
                className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
              >
                <option value="InProgress">�ang kh�m</option>
                <option value="Finalized">�� ch?t h? so</option>
              </select>
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">Tom tat ho so</label>
            <textarea
              rows={3}
              value={form.summary}
              onChange={(event) => setForm((current) => ({ ...current, summary: event.target.value }))}
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
              placeholder="Tom tat dien bien va ket luan chung"
            />
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <TextAreaField
              label="Trieu chung / Subjective"
              value={form.subjective}
              onChange={(value) => setForm((current) => ({ ...current, subjective: value }))}
            />
            <TextAreaField
              label="Kham thuc the / Objective"
              value={form.objective}
              onChange={(value) => setForm((current) => ({ ...current, objective: value }))}
            />
            <TextAreaField
              label="Danh gia / Assessment"
              value={form.assessment}
              onChange={(value) => setForm((current) => ({ ...current, assessment: value }))}
            />
            <TextAreaField
              label="Huong dieu tri / Care plan"
              value={form.carePlan}
              onChange={(value) => setForm((current) => ({ ...current, carePlan: value }))}
            />
          </div>

          <div className="grid gap-4 md:grid-cols-4">
            <NumberField label="Chieu cao (cm)" value={form.heightCm} onChange={(value) => setForm((current) => ({ ...current, heightCm: value }))} />
            <NumberField label="Can nang (kg)" value={form.weightKg} onChange={(value) => setForm((current) => ({ ...current, weightKg: value }))} />
            <NumberField label="Nhiet do (C)" value={form.temperatureC} onChange={(value) => setForm((current) => ({ ...current, temperatureC: value }))} />
            <NumberField label="Mach" value={form.pulseRate} onChange={(value) => setForm((current) => ({ ...current, pulseRate: value }))} />
            <NumberField label="Nhip tho" value={form.respiratoryRate} onChange={(value) => setForm((current) => ({ ...current, respiratoryRate: value }))} />
            <NumberField label="HA tam thu" value={form.systolicBp} onChange={(value) => setForm((current) => ({ ...current, systolicBp: value }))} />
            <NumberField label="HA tam truong" value={form.diastolicBp} onChange={(value) => setForm((current) => ({ ...current, diastolicBp: value }))} />
            <NumberField label="SpO2 (%)" value={form.oxygenSaturation} onChange={(value) => setForm((current) => ({ ...current, oxygenSaturation: value }))} />
          </div>

          <div className="flex justify-end gap-3 border-t border-slate-100 pt-5">
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                setIsModalOpen(false);
                setEditingEncounterId(null);
                setForm(EMPTY_FORM);
              }}
            >
              ��ng
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "�ang luu..." : editingEncounterId ? "C?p nh?t h? so" : "T?o h? so"}
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
  tone: "amber" | "emerald" | "cyan";
}) {
  const toneClasses = {
    amber: "border-amber-100 bg-amber-50/70 text-amber-700",
    emerald: "border-emerald-100 bg-emerald-50/70 text-emerald-700",
    cyan: "border-cyan-100 bg-cyan-50/70 text-cyan-700",
  };

  return (
    <Card className={`border p-5 shadow-sm hover:shadow-sm ${toneClasses[tone]}`}>
      <p className="text-xs font-bold uppercase tracking-[0.2em]">{label}</p>
      <p className="mt-3 text-3xl font-bold text-slate-950">{value}</p>
    </Card>
  );
}

function TextAreaField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <div>
      <label className="mb-2 block text-sm font-medium text-slate-700">{label}</label>
      <textarea
        rows={4}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
      />
    </div>
  );
}

function NumberField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <div>
      <label className="mb-2 block text-sm font-medium text-slate-700">{label}</label>
      <input
        type="number"
        step="0.1"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-emerald-500 focus:ring-4 focus:ring-emerald-100"
      />
    </div>
  );
}
