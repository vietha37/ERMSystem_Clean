"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Modal } from "@/components/ui/Modal";
import { getApiErrorMessage } from "@/services/error";
import { hospitalPrescriptionService } from "@/services/hospitalPrescriptionService";
import {
  CreateHospitalPrescriptionItemPayload,
  HospitalMedicineCatalog,
  HospitalPrescriptionDetail,
  HospitalPrescriptionEligibleEncounter,
  HospitalPrescriptionStatus,
  HospitalPrescriptionSummary,
} from "@/services/types";
import { useAuth } from "@/hooks/useAuth";
import toast from "react-hot-toast";

const STATUS_OPTIONS: Array<{
  value: HospitalPrescriptionStatus | "All";
  label: string;
}> = [
  { value: "All", label: "Tat ca" },
  { value: "Issued", label: "Da phat hanh" },
  { value: "Dispensed", label: "Da cap thuoc" },
  { value: "Cancelled", label: "Da huy" },
];

type PrescriptionItemForm = {
  medicineId: string;
  doseInstruction: string;
  route: string;
  frequency: string;
  durationDays: string;
  quantity: string;
};

const EMPTY_ITEM: PrescriptionItemForm = {
  medicineId: "",
  doseInstruction: "",
  route: "",
  frequency: "",
  durationDays: "",
  quantity: "1",
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

function getStatusLabel(status: HospitalPrescriptionStatus): string {
  switch (status) {
    case "Issued":
      return "Da phat hanh";
    case "Dispensed":
      return "Da cap thuoc";
    case "Cancelled":
      return "Da huy";
    default:
      return status;
  }
}

function getStatusClass(status: HospitalPrescriptionStatus): string {
  switch (status) {
    case "Issued":
      return "border border-cyan-200 bg-cyan-50 text-cyan-700";
    case "Dispensed":
      return "border border-emerald-200 bg-emerald-50 text-emerald-700";
    case "Cancelled":
      return "border border-rose-200 bg-rose-50 text-rose-700";
    default:
      return "border border-slate-200 bg-slate-100 text-slate-700";
  }
}

function buildCreateItems(items: PrescriptionItemForm[]): CreateHospitalPrescriptionItemPayload[] {
  return items
    .filter(
      (item) =>
        item.medicineId &&
        item.doseInstruction.trim() &&
        item.quantity.trim()
    )
    .map((item) => ({
      medicineId: item.medicineId,
      doseInstruction: item.doseInstruction.trim(),
      route: item.route.trim() || undefined,
      frequency: item.frequency.trim() || undefined,
      durationDays: item.durationDays.trim() ? Number(item.durationDays) : undefined,
      quantity: Number(item.quantity),
    }));
}

export default function PrescriptionsPage() {
  const { role } = useAuth();
  const [prescriptions, setPrescriptions] = useState<HospitalPrescriptionSummary[]>([]);
  const [eligibleEncounters, setEligibleEncounters] = useState<
    HospitalPrescriptionEligibleEncounter[]
  >([]);
  const [medicines, setMedicines] = useState<HospitalMedicineCatalog[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<HospitalPrescriptionStatus | "All">(
    "All"
  );
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedPrescription, setSelectedPrescription] =
    useState<HospitalPrescriptionDetail | null>(null);
  const [selectedEncounterId, setSelectedEncounterId] = useState("");
  const [notes, setNotes] = useState("");
  const [prescriptionItems, setPrescriptionItems] = useState<PrescriptionItemForm[]>([
    EMPTY_ITEM,
  ]);

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
        const [worklist, encounters, catalog] = await Promise.all([
          hospitalPrescriptionService.getAll({
            pageNumber,
            pageSize,
            status: statusFilter,
            textSearch: debouncedSearch || undefined,
          }),
          hospitalPrescriptionService.getEligibleEncounters(),
          hospitalPrescriptionService.getMedicineCatalog(),
        ]);

        setPrescriptions(worklist.items);
        setTotalCount(worklist.totalCount);
        setEligibleEncounters(encounters);
        setMedicines(catalog);
      } catch (error: unknown) {
        toast.error(getApiErrorMessage(error, "Khong the tai du lieu don thuoc."));
      } finally {
        setIsLoading(false);
        setIsRefreshing(false);
      }
    },
    [debouncedSearch, pageNumber, pageSize, statusFilter]
  );

  useEffect(() => {
    void fetchData();
  }, [fetchData]);

  const metrics = useMemo(
    () =>
      prescriptions.reduce(
        (acc, item) => {
          acc[item.status] += 1;
          return acc;
        },
        {
          Issued: 0,
          Dispensed: 0,
          Cancelled: 0,
        } as Record<HospitalPrescriptionStatus, number>
      ),
    [prescriptions]
  );

  const availableEncounters = eligibleEncounters.filter((item) => !item.existingPrescriptionId);
  const startItem = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const endItem = totalCount === 0 ? 0 : Math.min(pageNumber * pageSize, totalCount);

  const resetCreateForm = () => {
    setSelectedEncounterId("");
    setNotes("");
    setPrescriptionItems([EMPTY_ITEM]);
  };

  const handleAddRow = () => {
    setPrescriptionItems((current) => [...current, { ...EMPTY_ITEM }]);
  };

  const handleRemoveRow = (index: number) => {
    setPrescriptionItems((current) =>
      current.length === 1 ? current : current.filter((_, itemIndex) => itemIndex !== index)
    );
  };

  const handleUpdateRow = (
    index: number,
    field: keyof PrescriptionItemForm,
    value: string
  ) => {
    setPrescriptionItems((current) =>
      current.map((item, itemIndex) =>
        itemIndex === index ? { ...item, [field]: value } : item
      )
    );
  };

  const handleCreatePrescription = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!selectedEncounterId) {
      toast.error("Can chon encounter de phat hanh don thuoc.");
      return;
    }

    const items = buildCreateItems(prescriptionItems);
    if (items.length === 0) {
      toast.error("Can co it nhat mot thuoc hop le.");
      return;
    }

    setIsSubmitting(true);

    try {
      await hospitalPrescriptionService.create({
        encounterId: selectedEncounterId,
        status: "Issued",
        notes: notes.trim() || undefined,
        items,
      });

      toast.success("Da phat hanh don thuoc.");
      setIsCreateModalOpen(false);
      resetCreateForm();
      await fetchData(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Khong the tao don thuoc."));
    } finally {
      setIsSubmitting(false);
    }
  };

  const openDetail = async (prescriptionId: string) => {
    try {
      const detail = await hospitalPrescriptionService.getById(prescriptionId);
      setSelectedPrescription(detail);
      setIsDetailModalOpen(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Khong the tai chi tiet don thuoc."));
    }
  };

  const handleDelete = async (prescriptionId: string) => {
    if (role === "Doctor") {
      toast.error("Bac si khong co quyen xoa don thuoc.");
      return;
    }

    if (!confirm("Ban co chac muon xoa don thuoc nay?")) {
      return;
    }

    try {
      await hospitalPrescriptionService.delete(prescriptionId);
      toast.success("Da xoa don thuoc.");
      await fetchData(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Khong the xoa don thuoc."));
    }
  };

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <div className="rounded-[2rem] border border-cyan-100 bg-gradient-to-br from-cyan-50 via-white to-sky-50 p-6 shadow-sm">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.26em] text-cyan-700">
              Pharmacy service
            </p>
            <h1 className="mt-3 text-3xl font-bold text-slate-950">
              Don thuoc hospital
            </h1>
            <p className="mt-2 max-w-3xl text-sm leading-7 text-slate-600">
              Module don thuoc da chuyen sang hospital database moi, bám theo
              encounter va danh muc thuoc `pharmacy.Medicines`.
            </p>
          </div>

          <div className="flex flex-col gap-3 md:flex-row md:items-center">
            <input
              type="text"
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder="Tim theo ma don, encounter, benh nhan..."
              className="min-w-[260px] rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
            />

            <select
              value={statusFilter}
              onChange={(event) => {
                setStatusFilter(event.target.value as HospitalPrescriptionStatus | "All");
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

            <Button variant="secondary" onClick={() => void fetchData(true)} disabled={isRefreshing}>
              {isRefreshing ? "Dang lam moi..." : "Lam moi"}
            </Button>
            <Button onClick={() => setIsCreateModalOpen(true)}>Phat hanh don thuoc</Button>
          </div>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <MetricCard label="Da phat hanh" value={metrics.Issued} tone="cyan" />
        <MetricCard label="Da cap thuoc" value={metrics.Dispensed} tone="emerald" />
        <MetricCard label="Da huy" value={metrics.Cancelled} tone="rose" />
        <MetricCard label="Encounter cho ke don" value={availableEncounters.length} tone="amber" />
      </div>

      <Card className="overflow-hidden border border-slate-100 p-0 shadow-sm">
        <div className="flex flex-col gap-3 border-b border-slate-100 px-6 py-5 md:flex-row md:items-center md:justify-between">
          <div>
            <h2 className="text-lg font-bold text-slate-900">Danh sach don thuoc</h2>
            <p className="mt-1 text-sm text-slate-500">
              Hien thi {startItem}-{endItem} / {totalCount} don thuoc.
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
            <p className="text-sm font-medium text-slate-500">Dang tai don thuoc...</p>
          </div>
        ) : prescriptions.length === 0 ? (
          <div className="p-16 text-center text-sm text-slate-500">
            Chua co don thuoc nao khop bo loc hien tai.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-[1260px] w-full border-collapse text-left">
              <thead>
                <tr className="bg-slate-50">
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Don thuoc</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Benh nhan</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Bac si</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Chan doan</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Trang thai</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Ngay tao</th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">Tac vu</th>
                </tr>
              </thead>
              <tbody>
                {prescriptions.map((prescription) => (
                  <tr
                    key={prescription.prescriptionId}
                    className="border-t border-slate-100 align-top transition-colors hover:bg-cyan-50/30"
                  >
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{prescription.prescriptionNumber}</div>
                      <div className="mt-1 text-sm text-slate-500">{prescription.encounterNumber}</div>
                      <div className="mt-1 text-xs text-slate-400">{prescription.totalItems} thuoc</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{prescription.patientName}</div>
                      <div className="mt-1 text-sm text-slate-500">{prescription.medicalRecordNumber}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{prescription.doctorName}</div>
                      <div className="mt-1 text-sm text-slate-500">{prescription.specialtyName}</div>
                      <div className="mt-1 text-sm text-slate-500">{prescription.clinicName}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-cyan-700">
                        {prescription.primaryDiagnosisName || "--"}
                      </div>
                      <div className="mt-1 text-sm text-slate-500">
                        {prescription.notes || "Khong co ghi chu them."}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${getStatusClass(
                          prescription.status
                        )}`}
                      >
                        {getStatusLabel(prescription.status)}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-slate-600">
                      {formatDateTime(prescription.createdAtLocal)}
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-wrap gap-2">
                        <Button
                          variant="secondary"
                          className="border-cyan-200 text-cyan-700 hover:bg-cyan-50"
                          onClick={() => void openDetail(prescription.prescriptionId)}
                        >
                          Xem
                        </Button>
                        {role !== "Doctor" && (
                          <Button
                            variant="secondary"
                            className="border-rose-200 text-rose-700 hover:bg-rose-50"
                            onClick={() => void handleDelete(prescription.prescriptionId)}
                          >
                            Xoa
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
        isOpen={isCreateModalOpen}
        onClose={() => {
          setIsCreateModalOpen(false);
          resetCreateForm();
        }}
        title="Phat hanh don thuoc"
      >
        <form onSubmit={handleCreatePrescription} className="space-y-5">
          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Chon encounter
            </label>
            <select
              value={selectedEncounterId}
              onChange={(event) => setSelectedEncounterId(event.target.value)}
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
              required
            >
              <option value="">-- Chon encounter --</option>
              {availableEncounters.map((encounter) => (
                <option key={encounter.encounterId} value={encounter.encounterId}>
                  {encounter.encounterNumber} - {encounter.patientName} - {encounter.primaryDiagnosisName || "Chua co chan doan"}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">Ghi chu</label>
            <textarea
              rows={3}
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
              placeholder="Luu y su dung thuoc, huong dan bo sung..."
            />
          </div>

          <div className="space-y-4 border-t border-slate-100 pt-5">
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-bold text-slate-800">Danh sach thuoc</h3>
              <Button type="button" variant="secondary" onClick={handleAddRow}>
                Them dong
              </Button>
            </div>

            {prescriptionItems.map((item, index) => (
              <div
                key={index}
                className="rounded-2xl border border-dashed border-slate-200 bg-slate-50/70 p-4"
              >
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="md:col-span-2">
                    <label className="mb-2 block text-sm font-medium text-slate-700">Thuoc</label>
                    <select
                      value={item.medicineId}
                      onChange={(event) => handleUpdateRow(index, "medicineId", event.target.value)}
                      className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
                      required
                    >
                      <option value="">-- Chon thuoc --</option>
                      {medicines.map((medicine) => (
                        <option key={medicine.medicineId} value={medicine.medicineId}>
                          {medicine.name} ({medicine.drugCode}) {medicine.strength ? `- ${medicine.strength}` : ""}
                        </option>
                      ))}
                    </select>
                  </div>

                  <InputField
                    label="Lieu dung"
                    value={item.doseInstruction}
                    onChange={(value) => handleUpdateRow(index, "doseInstruction", value)}
                    placeholder="1 vien/lần"
                  />
                  <InputField
                    label="Duong dung"
                    value={item.route}
                    onChange={(value) => handleUpdateRow(index, "route", value)}
                    placeholder="Uong"
                  />
                  <InputField
                    label="Tan suat"
                    value={item.frequency}
                    onChange={(value) => handleUpdateRow(index, "frequency", value)}
                    placeholder="Ngay 2 lan"
                  />
                  <InputField
                    label="So ngay"
                    value={item.durationDays}
                    onChange={(value) => handleUpdateRow(index, "durationDays", value)}
                    placeholder="5"
                    type="number"
                  />
                  <InputField
                    label="So luong"
                    value={item.quantity}
                    onChange={(value) => handleUpdateRow(index, "quantity", value)}
                    placeholder="10"
                    type="number"
                  />
                </div>

                <div className="mt-4 flex justify-end">
                  <Button
                    type="button"
                    variant="secondary"
                    className="border-rose-200 text-rose-700 hover:bg-rose-50"
                    onClick={() => handleRemoveRow(index)}
                    disabled={prescriptionItems.length === 1}
                  >
                    Xoa dong
                  </Button>
                </div>
              </div>
            ))}
          </div>

          <div className="flex justify-end gap-3 border-t border-slate-100 pt-5">
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                setIsCreateModalOpen(false);
                resetCreateForm();
              }}
            >
              Dong
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Dang phat hanh..." : "Xac nhan don thuoc"}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isDetailModalOpen}
        onClose={() => {
          setIsDetailModalOpen(false);
          setSelectedPrescription(null);
        }}
        title="Chi tiet don thuoc"
      >
        {selectedPrescription ? (
          <div className="space-y-5">
            <div className="grid gap-3 rounded-2xl border border-cyan-100 bg-cyan-50/70 p-4 text-sm md:grid-cols-2">
              <InfoLine label="Don thuoc" value={selectedPrescription.prescriptionNumber} />
              <InfoLine label="Encounter" value={selectedPrescription.encounterNumber} />
              <InfoLine label="Benh nhan" value={selectedPrescription.patientName} />
              <InfoLine label="Bac si" value={selectedPrescription.doctorName} />
              <InfoLine label="Chan doan" value={selectedPrescription.primaryDiagnosisName || "--"} />
              <InfoLine label="Ngay tao" value={formatDateTime(selectedPrescription.createdAtLocal)} />
            </div>

            <div className="overflow-hidden rounded-2xl border border-slate-100">
              <table className="w-full border-collapse text-left text-sm">
                <thead className="bg-slate-50">
                  <tr>
                    <th className="px-4 py-3 font-semibold text-slate-700">Thuoc</th>
                    <th className="px-4 py-3 font-semibold text-slate-700">Lieu dung</th>
                    <th className="px-4 py-3 font-semibold text-slate-700">Tan suat</th>
                    <th className="px-4 py-3 font-semibold text-slate-700">So luong</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedPrescription.items.map((item) => (
                    <tr key={item.prescriptionItemId} className="border-t border-slate-100">
                      <td className="px-4 py-3">
                        <div className="font-semibold text-slate-900">{item.medicineName}</div>
                        <div className="mt-1 text-xs text-slate-500">
                          {item.drugCode}
                          {item.strength ? ` / ${item.strength}` : ""}
                          {item.dosageForm ? ` / ${item.dosageForm}` : ""}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-slate-600">{item.doseInstruction}</td>
                      <td className="px-4 py-3 text-slate-600">
                        {[item.route, item.frequency, item.durationDays ? `${item.durationDays} ngay` : null]
                          .filter(Boolean)
                          .join(" / ") || "--"}
                      </td>
                      <td className="px-4 py-3 text-slate-600">
                        {item.quantity} {item.unit || ""}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex justify-end">
              <Button
                variant="secondary"
                onClick={() => {
                  setIsDetailModalOpen(false);
                  setSelectedPrescription(null);
                }}
              >
                Dong
              </Button>
            </div>
          </div>
        ) : null}
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
  tone: "cyan" | "emerald" | "rose" | "amber";
}) {
  const toneClasses = {
    cyan: "border-cyan-100 bg-cyan-50/70 text-cyan-700",
    emerald: "border-emerald-100 bg-emerald-50/70 text-emerald-700",
    rose: "border-rose-100 bg-rose-50/70 text-rose-700",
    amber: "border-amber-100 bg-amber-50/70 text-amber-700",
  };

  return (
    <Card className={`border p-5 shadow-sm hover:shadow-sm ${toneClasses[tone]}`}>
      <p className="text-xs font-bold uppercase tracking-[0.2em]">{label}</p>
      <p className="mt-3 text-3xl font-bold text-slate-950">{value}</p>
    </Card>
  );
}

function InputField({
  label,
  value,
  onChange,
  placeholder,
  type = "text",
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  type?: "text" | "number";
}) {
  return (
    <div>
      <label className="mb-2 block text-sm font-medium text-slate-700">{label}</label>
      <input
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
      />
    </div>
  );
}

function InfoLine({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-slate-500">{label}</p>
      <p className="mt-1 font-semibold text-slate-900">{value}</p>
    </div>
  );
}
