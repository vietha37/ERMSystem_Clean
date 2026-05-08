"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Modal } from "@/components/ui/Modal";
import { getApiErrorMessage } from "@/services/error";
import { hospitalClinicalOrderService } from "@/services/hospitalClinicalOrderService";
import {
  CreateHospitalClinicalOrderPayload,
  HospitalClinicalOrderCategory,
  HospitalClinicalOrderCatalogItem,
  HospitalClinicalOrderDetail,
  HospitalClinicalOrderEligibleEncounter,
  HospitalClinicalOrderStatus,
  HospitalClinicalOrderSummary,
  RecordHospitalLabResultItemPayload,
} from "@/services/types";
import toast from "react-hot-toast";

const CATEGORY_OPTIONS: Array<{
  value: HospitalClinicalOrderCategory | "All";
  label: string;
}> = [
  { value: "All", label: "Tất cả" },
  { value: "Lab", label: "Xét nghiệm" },
  { value: "Imaging", label: "Chẩn đoán hình ảnh" },
];

const STATUS_OPTIONS: Array<{
  value: HospitalClinicalOrderStatus | "All";
  label: string;
}> = [
  { value: "All", label: "Tất cả" },
  { value: "Requested", label: "Đang chờ kết quả" },
  { value: "Completed", label: "Đã hoàn thành" },
];

const EMPTY_LAB_RESULT_ITEM: RecordHospitalLabResultItemPayload = {
  analyteCode: "",
  analyteName: "",
  resultValue: "",
  unit: "",
  referenceRange: "",
  abnormalFlag: "",
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

function getCategoryLabel(category: HospitalClinicalOrderCategory): string {
  return category === "Lab" ? "Xét nghiệm" : "Chẩn đoán hình ảnh";
}

function getCategoryClass(category: HospitalClinicalOrderCategory): string {
  return category === "Lab"
    ? "border border-cyan-200 bg-cyan-50 text-cyan-700"
    : "border border-violet-200 bg-violet-50 text-violet-700";
}

function getStatusLabel(status: HospitalClinicalOrderStatus): string {
  return status === "Requested" ? "Đang chờ kết quả" : "Đã hoàn thành";
}

function getStatusClass(status: HospitalClinicalOrderStatus): string {
  return status === "Requested"
    ? "border border-amber-200 bg-amber-50 text-amber-700"
    : "border border-emerald-200 bg-emerald-50 text-emerald-700";
}

function buildCreatePayload(
  encounterId: string,
  category: HospitalClinicalOrderCategory,
  serviceId: string,
  priorityCode: string
): CreateHospitalClinicalOrderPayload {
  return {
    encounterId,
    category,
    serviceId,
    priorityCode: category === "Lab" && priorityCode.trim() ? priorityCode.trim() : undefined,
  };
}

function buildLabResultItems(items: RecordHospitalLabResultItemPayload[]) {
  return items
    .filter((item) => item.analyteName.trim())
    .map((item) => ({
      analyteCode: item.analyteCode?.trim() || undefined,
      analyteName: item.analyteName.trim(),
      resultValue: item.resultValue?.trim() || undefined,
      unit: item.unit?.trim() || undefined,
      referenceRange: item.referenceRange?.trim() || undefined,
      abnormalFlag: item.abnormalFlag?.trim() || undefined,
    }));
}

export default function ClinicalOrdersPage() {
  const [orders, setOrders] = useState<HospitalClinicalOrderSummary[]>([]);
  const [eligibleEncounters, setEligibleEncounters] = useState<
    HospitalClinicalOrderEligibleEncounter[]
  >([]);
  const [catalog, setCatalog] = useState<HospitalClinicalOrderCatalogItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<
    HospitalClinicalOrderCategory | "All"
  >("All");
  const [statusFilter, setStatusFilter] = useState<HospitalClinicalOrderStatus | "All">(
    "All"
  );
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [isLabResultModalOpen, setIsLabResultModalOpen] = useState(false);
  const [isImagingReportModalOpen, setIsImagingReportModalOpen] = useState(false);

  const [selectedDetail, setSelectedDetail] = useState<HospitalClinicalOrderDetail | null>(
    null
  );
  const [labResultTarget, setLabResultTarget] = useState<HospitalClinicalOrderSummary | null>(
    null
  );
  const [imagingReportTarget, setImagingReportTarget] =
    useState<HospitalClinicalOrderSummary | null>(null);

  const [selectedEncounterId, setSelectedEncounterId] = useState("");
  const [createCategory, setCreateCategory] = useState<HospitalClinicalOrderCategory>("Lab");
  const [selectedServiceId, setSelectedServiceId] = useState("");
  const [priorityCode, setPriorityCode] = useState("Routine");

  const [specimenCode, setSpecimenCode] = useState("");
  const [labResultItems, setLabResultItems] = useState<RecordHospitalLabResultItemPayload[]>([
    EMPTY_LAB_RESULT_ITEM,
  ]);
  const [findings, setFindings] = useState("");
  const [impression, setImpression] = useState("");
  const [reportUri, setReportUri] = useState("");

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
        const [worklist, encounterData, catalogData] = await Promise.all([
          hospitalClinicalOrderService.getAll({
            pageNumber,
            pageSize,
            category: categoryFilter,
            status: statusFilter,
            textSearch: debouncedSearch || undefined,
          }),
          hospitalClinicalOrderService.getEligibleEncounters(),
          hospitalClinicalOrderService.getCatalog(),
        ]);

        setOrders(worklist.items);
        setTotalCount(worklist.totalCount);
        setEligibleEncounters(encounterData);
        setCatalog(catalogData);
      } catch (error: unknown) {
        toast.error(getApiErrorMessage(error, "Kh�ng th? t?i d? li?u ch? d?nh c?n l�m s�ng."));
      } finally {
        setIsLoading(false);
        setIsRefreshing(false);
      }
    },
    [categoryFilter, debouncedSearch, pageNumber, pageSize, statusFilter]
  );

  useEffect(() => {
    void fetchData();
  }, [fetchData]);

  const metrics = useMemo(
    () =>
      orders.reduce(
        (acc, item) => {
          acc.total += 1;
          acc[item.category] += 1;
          acc[item.status] += 1;
          return acc;
        },
        {
          total: 0,
          Lab: 0,
          Imaging: 0,
          Requested: 0,
          Completed: 0,
        } as Record<"total" | HospitalClinicalOrderCategory | HospitalClinicalOrderStatus, number>
      ),
    [orders]
  );

  const availableServices = useMemo(
    () => catalog.filter((item) => item.category === createCategory),
    [catalog, createCategory]
  );

  const availableEncounters = useMemo(() => eligibleEncounters, [eligibleEncounters]);

  useEffect(() => {
    setSelectedServiceId((current) => {
      if (availableServices.some((item) => item.serviceId === current)) {
        return current;
      }

      return availableServices[0]?.serviceId ?? "";
    });
  }, [availableServices]);

  const resetCreateForm = () => {
    setSelectedEncounterId("");
    setCreateCategory("Lab");
    setSelectedServiceId("");
    setPriorityCode("Routine");
  };

  const resetLabResultForm = () => {
    setSpecimenCode("");
    setLabResultItems([EMPTY_LAB_RESULT_ITEM]);
  };

  const resetImagingReportForm = () => {
    setFindings("");
    setImpression("");
    setReportUri("");
  };

  const startItem = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const endItem = totalCount === 0 ? 0 : Math.min(pageNumber * pageSize, totalCount);

  const openDetail = async (clinicalOrderId: string) => {
    try {
      const detail = await hospitalClinicalOrderService.getById(clinicalOrderId);
      setSelectedDetail(detail);
      setIsDetailModalOpen(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Kh�ng th? t?i chi ti?t ch? d?nh."));
    }
  };

  const handleCreateOrder = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!selectedEncounterId) {
      toast.error("Can chon encounter de tao chi dinh.");
      return;
    }

    if (!selectedServiceId) {
      toast.error("Can chon dich vu can lam sang.");
      return;
    }

    setIsSubmitting(true);

    try {
      await hospitalClinicalOrderService.create(
        buildCreatePayload(selectedEncounterId, createCategory, selectedServiceId, priorityCode)
      );

      toast.success("�� t?o ch? d?nh c?n l�m s�ng.");
      setIsCreateModalOpen(false);
      resetCreateForm();
      await fetchData(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Kh�ng th? t?o ch? d?nh."));
    } finally {
      setIsSubmitting(false);
    }
  };

  const openLabResultModal = (order: HospitalClinicalOrderSummary) => {
    setLabResultTarget(order);
    resetLabResultForm();
    setIsLabResultModalOpen(true);
  };

  const handleAddLabResultRow = () => {
    setLabResultItems((current) => [...current, { ...EMPTY_LAB_RESULT_ITEM }]);
  };

  const handleRemoveLabResultRow = (index: number) => {
    setLabResultItems((current) =>
      current.length === 1 ? current : current.filter((_, itemIndex) => itemIndex !== index)
    );
  };

  const handleUpdateLabResultRow = (
    index: number,
    field: keyof RecordHospitalLabResultItemPayload,
    value: string
  ) => {
    setLabResultItems((current) =>
      current.map((item, itemIndex) =>
        itemIndex === index ? { ...item, [field]: value } : item
      )
    );
  };

  const handleRecordLabResult = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!labResultTarget) {
      toast.error("Kh�ng x�c d?nh duoc chi dinh xet nghiem.");
      return;
    }

    const resultItems = buildLabResultItems(labResultItems);
    if (resultItems.length === 0) {
      toast.error("Can co it nhat mot dong ket qua xet nghiem hop le.");
      return;
    }

    setIsSubmitting(true);

    try {
      const updated = await hospitalClinicalOrderService.recordLabResult(
        labResultTarget.clinicalOrderId,
        {
          specimenCode: specimenCode.trim() || undefined,
          resultItems,
        }
      );

      toast.success("�� ghi nh?n k?t qu? x�t nghi?m.");
      setIsLabResultModalOpen(false);
      setLabResultTarget(null);
      resetLabResultForm();
      if (selectedDetail?.clinicalOrderId === updated.clinicalOrderId) {
        setSelectedDetail(updated);
      }
      await fetchData(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Kh�ng th? ghi nh?n k?t qu? x�t nghi?m."));
    } finally {
      setIsSubmitting(false);
    }
  };

  const openImagingReportModal = (order: HospitalClinicalOrderSummary) => {
    setImagingReportTarget(order);
    resetImagingReportForm();
    setIsImagingReportModalOpen(true);
  };

  const handleRecordImagingReport = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!imagingReportTarget) {
      toast.error("Kh�ng x�c d?nh duoc chi dinh chan doan hinh anh.");
      return;
    }

    if (!findings.trim() && !impression.trim()) {
      toast.error("Can co findings hoac impression de luu bao cao.");
      return;
    }

    setIsSubmitting(true);

    try {
      const updated = await hospitalClinicalOrderService.recordImagingReport(
        imagingReportTarget.clinicalOrderId,
        {
          findings: findings.trim() || undefined,
          impression: impression.trim() || undefined,
          reportUri: reportUri.trim() || undefined,
        }
      );

      toast.success("�� ghi nh?n b�o c�o ch?n do�n h�nh ?nh.");
      setIsImagingReportModalOpen(false);
      setImagingReportTarget(null);
      resetImagingReportForm();
      if (selectedDetail?.clinicalOrderId === updated.clinicalOrderId) {
        setSelectedDetail(updated);
      }
      await fetchData(true);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Kh�ng th? ghi nh?n b�o c�o."));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <div className="rounded-[2rem] border border-violet-100 bg-gradient-to-br from-violet-50 via-white to-cyan-50 p-6 shadow-sm">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.26em] text-violet-700">
              Clinical orders
            </p>
            <h1 className="mt-3 text-3xl font-bold text-slate-950">
              Ch? d?nh c?n l�m s�ng
            </h1>
            <p className="mt-2 max-w-3xl text-sm leading-7 text-slate-600">
              Qu?n l� worklist x�t nghi?m v� ch?n do�n h�nh ?nh theo hospital database m?i,
              b�m theo encounter v� order header.
            </p>
          </div>

          <div className="flex flex-col gap-3 md:flex-row md:items-center">
            <input
              type="text"
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder="Tim theo ma chi dinh, benh nhan, dich vu..."
              className="min-w-[260px] rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
            />

            <select
              value={categoryFilter}
              onChange={(event) => {
                setCategoryFilter(
                  event.target.value as HospitalClinicalOrderCategory | "All"
                );
                setPageNumber(1);
              }}
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
            >
              {CATEGORY_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              value={statusFilter}
              onChange={(event) => {
                setStatusFilter(event.target.value as HospitalClinicalOrderStatus | "All");
                setPageNumber(1);
              }}
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
            >
              {STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <Button variant="secondary" onClick={() => void fetchData(true)} disabled={isRefreshing}>
              {isRefreshing ? "�ang l�m m?i..." : "Lam moi"}
            </Button>
            <Button onClick={() => setIsCreateModalOpen(true)}>Tao chi dinh</Button>
          </div>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
        <MetricCard label="Tá»ng chá» Äá»nh" value={metrics.total} tone="slate" />
        <MetricCard label="X�t nghi?m" value={metrics.Lab} tone="cyan" />
        <MetricCard label="Ch?n do�n hinh anh" value={metrics.Imaging} tone="violet" />
        <MetricCard label="�ang ch? ket qua" value={metrics.Requested} tone="amber" />
        <MetricCard label="�� ho�n th�nh" value={metrics.Completed} tone="emerald" />
      </div>

      <Card className="overflow-hidden border border-slate-100 p-0 shadow-sm">
        <div className="flex flex-col gap-3 border-b border-slate-100 px-6 py-5 md:flex-row md:items-center md:justify-between">
          <div>
            <h2 className="text-lg font-bold text-slate-900">Danh sach chi dinh</h2>
            <p className="mt-1 text-sm text-slate-500">
              Hien thi {startItem}-{endItem} / {totalCount} chi dinh.
            </p>
          </div>

          <select
            value={pageSize}
            onChange={(event) => {
              setPageSize(Number(event.target.value));
              setPageNumber(1);
            }}
            className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
          >
            <option value={10}>10 dong / trang</option>
            <option value={20}>20 dong / trang</option>
            <option value={50}>50 dong / trang</option>
          </select>
        </div>

        {isLoading ? (
          <div className="flex flex-col items-center justify-center p-16">
            <div className="mb-4 h-10 w-10 animate-spin rounded-full border-4 border-violet-100 border-t-violet-600" />
            <p className="text-sm font-medium text-slate-500">
              �ang t?i worklist chi dinh...
            </p>
          </div>
        ) : orders.length === 0 ? (
          <div className="p-16 text-center text-sm text-slate-500">
            Chua co chi dinh nao khop bo loc hien tai.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-[1280px] w-full border-collapse text-left">
              <thead>
                <tr className="bg-slate-50">
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                    Thoi gian
                  </th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                    B?nh nh�n
                  </th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                    Dich vu
                  </th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                    Phan loai
                  </th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                    Tom tat
                  </th>
                  <th className="px-6 py-4 text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                    Thao tac
                  </th>
                </tr>
              </thead>
              <tbody>
                {orders.map((order) => (
                  <tr
                    key={order.clinicalOrderId}
                    className="border-t border-slate-100 align-top transition-colors hover:bg-violet-50/20"
                  >
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">
                        {formatDateTime(order.requestedAtLocal)}
                      </div>
                      <div className="mt-1 text-sm text-slate-500">{order.orderNumber}</div>
                      <div className="mt-1 text-xs text-slate-400">
                        {order.orderedByUsername || "Kh�ng r� ngu?i ch? d?nh"}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">{order.patientName}</div>
                      <div className="mt-1 text-sm text-slate-500">
                        {order.medicalRecordNumber}
                      </div>
                      <div className="mt-1 text-xs text-slate-400">{order.encounterNumber}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="font-semibold text-slate-900">
                        {order.serviceName}
                      </div>
                      <div className="mt-1 text-sm text-slate-500">{order.serviceCode}</div>
                      <div className="mt-1 text-xs text-slate-400">
                        {order.doctorName} / {order.clinicName}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-col gap-2">
                        <span
                          className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-bold ${getCategoryClass(order.category)}`}
                        >
                          {getCategoryLabel(order.category)}
                        </span>
                        <span
                          className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-bold ${getStatusClass(order.status)}`}
                        >
                          {getStatusLabel(order.status)}
                        </span>
                        {order.priorityCode && (
                          <span className="text-xs text-slate-500">
                            Uu tien: {order.priorityCode}
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="max-w-[320px] text-sm leading-6 text-slate-600">
                        {order.summaryText || "Chua co noi dung ket qua."}
                      </div>
                      <div className="mt-2 text-xs text-slate-400">
                        So dong ket qua: {order.resultItemCount}
                      </div>
                      <div className="mt-1 text-xs text-slate-400">
                        Hoan thanh: {formatDateTime(order.completedAtLocal)}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-col gap-2">
                        <Button
                          variant="secondary"
                          className="justify-start"
                          onClick={() => void openDetail(order.clinicalOrderId)}
                        >
                          Xem chi tiet
                        </Button>
                        {order.status !== "Completed" && order.category === "Lab" && (
                          <Button
                            className="justify-start"
                            onClick={() => openLabResultModal(order)}
                          >
                            Nh?p k?t qu?
                          </Button>
                        )}
                        {order.status !== "Completed" && order.category === "Imaging" && (
                          <Button
                            className="justify-start"
                            onClick={() => openImagingReportModal(order)}
                          >
                            Nh?p b�o c�o
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

        <div className="flex flex-col gap-4 border-t border-slate-100 px-6 py-5 md:flex-row md:items-center md:justify-between">
          <div className="text-sm text-slate-500">
            Trang {pageNumber} / {totalPages}
          </div>

          <div className="flex gap-3">
            <Button
              variant="secondary"
              disabled={pageNumber <= 1}
              onClick={() => setPageNumber((current) => Math.max(1, current - 1))}
            >
              Trang truoc
            </Button>
            <Button
              variant="secondary"
              disabled={pageNumber >= totalPages}
              onClick={() =>
                setPageNumber((current) => Math.min(totalPages, current + 1))
              }
            >
              Trang sau
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
        title="T?o ch? d?nh c?n l�m s�ng"
      >
        <form className="space-y-4" onSubmit={handleCreateOrder}>
          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-700">
              Encounter
            </label>
            <select
              value={selectedEncounterId}
              onChange={(event) => setSelectedEncounterId(event.target.value)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
            >
              <option value="">Ch?n encounter</option>
              {availableEncounters.map((encounter) => (
                <option key={encounter.encounterId} value={encounter.encounterId}>
                  {encounter.encounterNumber} - {encounter.patientName} -{" "}
                  {encounter.primaryDiagnosisName || encounter.specialtyName}
                </option>
              ))}
            </select>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="mb-2 block text-sm font-semibold text-slate-700">
                Lo?i ch? d?nh
              </label>
              <select
                value={createCategory}
                onChange={(event) =>
                  setCreateCategory(event.target.value as HospitalClinicalOrderCategory)
                }
                className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
              >
                <option value="Lab">X�t nghi?m</option>
                <option value="Imaging">Ch?n do�n hinh anh</option>
              </select>
            </div>

            <div>
              <label className="mb-2 block text-sm font-semibold text-slate-700">
                M?c uu ti�n
              </label>
              <input
                type="text"
                value={priorityCode}
                onChange={(event) => setPriorityCode(event.target.value)}
                disabled={createCategory !== "Lab"}
                placeholder="Routine"
                className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100 disabled:bg-slate-100"
              />
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-700">
              Dich vu
            </label>
            <select
              value={selectedServiceId}
              onChange={(event) => setSelectedServiceId(event.target.value)}
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
            >
              <option value="">Ch?n d?ch v?</option>
              {availableServices.map((service) => (
                <option key={service.serviceId} value={service.serviceId}>
                  {service.serviceCode} - {service.serviceName}
                  {service.extraLabel ? ` (${service.extraLabel})` : ""}
                </option>
              ))}
            </select>
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                setIsCreateModalOpen(false);
                resetCreateForm();
              }}
            >
              ��ng
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "�ang t?o..." : "Luu ch? d?nh"}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isDetailModalOpen}
        onClose={() => {
          setIsDetailModalOpen(false);
          setSelectedDetail(null);
        }}
        title="Chi tiet chi dinh"
      >
        {!selectedDetail ? (
          <div className="py-8 text-sm text-slate-500">�ang t?i chi tiet...</div>
        ) : (
          <div className="space-y-4">
            <div className="grid gap-3 md:grid-cols-2">
              <InfoTile label="Ma chi dinh" value={selectedDetail.orderNumber} />
              <InfoTile
                label="Loai"
                value={getCategoryLabel(selectedDetail.category)}
              />
              <InfoTile label="B?nh nh�n" value={selectedDetail.patientName} />
              <InfoTile label="Ma benh an" value={selectedDetail.medicalRecordNumber} />
              <InfoTile label="Encounter" value={selectedDetail.encounterNumber} />
              <InfoTile label="Dich vu" value={selectedDetail.serviceName} />
              <InfoTile label="Trang thai" value={getStatusLabel(selectedDetail.status)} />
              <InfoTile
                label="Hoan thanh"
                value={formatDateTime(selectedDetail.completedAtLocal)}
              />
            </div>

            {selectedDetail.category === "Lab" ? (
              <div className="space-y-3">
                <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                  <div className="text-sm font-semibold text-slate-800">Th�ng tin m?u</div>
                  <div className="mt-2 grid gap-2 text-sm text-slate-600 md:grid-cols-2">
                    <div>M� m?u: {selectedDetail.specimenCode || "--"}</div>
                    <div>Trang thai: {selectedDetail.specimenStatus || "--"}</div>
                    <div>Lay mau: {formatDateTime(selectedDetail.collectedAtLocal)}</div>
                    <div>Tiep nhan: {formatDateTime(selectedDetail.receivedAtLocal)}</div>
                  </div>
                </div>

                <div>
                  <div className="mb-2 text-sm font-semibold text-slate-800">
                    K?t qu? x�t nghi?m
                  </div>
                  {selectedDetail.resultItems.length === 0 ? (
                    <div className="rounded-2xl border border-dashed border-slate-200 px-4 py-6 text-sm text-slate-500">
                      Chua c� d�ng k?t qu? n�o.
                    </div>
                  ) : (
                    <div className="space-y-2">
                      {selectedDetail.resultItems.map((item) => (
                        <div
                          key={item.resultItemId}
                          className="rounded-2xl border border-slate-100 px-4 py-3"
                        >
                          <div className="font-semibold text-slate-900">
                            {item.analyteName}
                          </div>
                          <div className="mt-1 text-sm text-slate-600">
                            {item.resultValue || "--"} {item.unit || ""}
                          </div>
                          <div className="mt-1 text-xs text-slate-400">
                            Ref: {item.referenceRange || "--"} | Bat thuong:{" "}
                            {item.abnormalFlag || "--"}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="space-y-3">
                <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                  <div className="text-sm font-semibold text-slate-800">Findings</div>
                  <div className="mt-2 whitespace-pre-line text-sm leading-6 text-slate-600">
                    {selectedDetail.findings || "--"}
                  </div>
                </div>
                <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
                  <div className="text-sm font-semibold text-slate-800">Impression</div>
                  <div className="mt-2 whitespace-pre-line text-sm leading-6 text-slate-600">
                    {selectedDetail.impression || "--"}
                  </div>
                </div>
                <div className="grid gap-3 md:grid-cols-2">
                  <InfoTile label="Nguoi ky" value={selectedDetail.signedByUsername || "--"} />
                  <InfoTile
                    label="Thoi diem ky"
                    value={formatDateTime(selectedDetail.signedAtLocal)}
                  />
                  <InfoTile label="Li�n k?t b�o c�o" value={selectedDetail.reportUri || "--"} />
                  <InfoTile
                    label="Tom tat"
                    value={selectedDetail.summaryText || "--"}
                  />
                </div>
              </div>
            )}
          </div>
        )}
      </Modal>

      <Modal
        isOpen={isLabResultModalOpen}
        onClose={() => {
          setIsLabResultModalOpen(false);
          setLabResultTarget(null);
          resetLabResultForm();
        }}
        title="Nh?p k?t qu? xet nghiem"
      >
        <form className="space-y-4" onSubmit={handleRecordLabResult}>
          {labResultTarget && (
            <div className="rounded-2xl border border-cyan-100 bg-cyan-50 px-4 py-3 text-sm text-cyan-800">
              {labResultTarget.orderNumber} - {labResultTarget.patientName} -{" "}
              {labResultTarget.serviceName}
            </div>
          )}

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-700">
              M� m?u
            </label>
            <input
              type="text"
              value={specimenCode}
              onChange={(event) => setSpecimenCode(event.target.value)}
              placeholder="�? tr?ng d? h? th?ng t? sinh"
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
            />
          </div>

          <div className="space-y-3">
            {labResultItems.map((item, index) => (
              <div key={`${index}-${item.analyteCode ?? ""}`} className="rounded-2xl border border-slate-100 p-4">
                <div className="mb-3 flex items-center justify-between">
                  <div className="text-sm font-semibold text-slate-800">
                    ��ng ket qua {index + 1}
                  </div>
                  <button
                    type="button"
                    onClick={() => handleRemoveLabResultRow(index)}
                    className="text-sm font-medium text-rose-600"
                  >
                    X�a d�ng
                  </button>
                </div>

                <div className="grid gap-3 md:grid-cols-2">
                  <input
                    type="text"
                    value={item.analyteName}
                    onChange={(event) =>
                      handleUpdateLabResultRow(index, "analyteName", event.target.value)
                    }
                    placeholder="T�n ch? s?"
                    className="rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
                  />
                  <input
                    type="text"
                    value={item.analyteCode ?? ""}
                    onChange={(event) =>
                      handleUpdateLabResultRow(index, "analyteCode", event.target.value)
                    }
                    placeholder="M� ch? s?"
                    className="rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
                  />
                  <input
                    type="text"
                    value={item.resultValue ?? ""}
                    onChange={(event) =>
                      handleUpdateLabResultRow(index, "resultValue", event.target.value)
                    }
                    placeholder="Gia tri"
                    className="rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
                  />
                  <input
                    type="text"
                    value={item.unit ?? ""}
                    onChange={(event) =>
                      handleUpdateLabResultRow(index, "unit", event.target.value)
                    }
                    placeholder="�on v?"
                    className="rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
                  />
                  <input
                    type="text"
                    value={item.referenceRange ?? ""}
                    onChange={(event) =>
                      handleUpdateLabResultRow(index, "referenceRange", event.target.value)
                    }
                    placeholder="Kho?ng tham chi?u"
                    className="rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
                  />
                  <input
                    type="text"
                    value={item.abnormalFlag ?? ""}
                    onChange={(event) =>
                      handleUpdateLabResultRow(index, "abnormalFlag", event.target.value)
                    }
                    placeholder="C?nh b�o b?t thu?ng"
                    className="rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-cyan-500 focus:ring-4 focus:ring-cyan-100"
                  />
                </div>
              </div>
            ))}
          </div>

          <Button type="button" variant="secondary" onClick={handleAddLabResultRow}>
            Th�m d�ng k?t qu?
          </Button>

          <div className="flex justify-end gap-3 pt-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                setIsLabResultModalOpen(false);
                setLabResultTarget(null);
                resetLabResultForm();
              }}
            >
              ��ng
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "�ang luu..." : "Luu k?t qu?"}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isImagingReportModalOpen}
        onClose={() => {
          setIsImagingReportModalOpen(false);
          setImagingReportTarget(null);
          resetImagingReportForm();
        }}
        title="Nh?p b�o c�o chan doan hinh anh"
      >
        <form className="space-y-4" onSubmit={handleRecordImagingReport}>
          {imagingReportTarget && (
            <div className="rounded-2xl border border-violet-100 bg-violet-50 px-4 py-3 text-sm text-violet-800">
              {imagingReportTarget.orderNumber} - {imagingReportTarget.patientName} -{" "}
              {imagingReportTarget.serviceName}
            </div>
          )}

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-700">
              Findings
            </label>
            <textarea
              value={findings}
              onChange={(event) => setFindings(event.target.value)}
              rows={5}
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
              placeholder="M� t? chi ti?t k?t qu? ch?n do�n h�nh ?nh..."
            />
          </div>

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-700">
              Impression
            </label>
            <textarea
              value={impression}
              onChange={(event) => setImpression(event.target.value)}
              rows={4}
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
              placeholder="K?t lu?n ch?n do�n..."
            />
          </div>

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-700">
              Li�n k?t b�o c�o
            </label>
            <input
              type="text"
              value={reportUri}
              onChange={(event) => setReportUri(event.target.value)}
              placeholder="https://... hoac duong dan noi bo"
              className="w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-violet-500 focus:ring-4 focus:ring-violet-100"
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => {
                setIsImagingReportModalOpen(false);
                setImagingReportTarget(null);
                resetImagingReportForm();
              }}
            >
              ��ng
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "�ang luu..." : "Luu b�o c�o"}
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
  tone: "slate" | "cyan" | "violet" | "amber" | "emerald";
}) {
  const toneClass = {
    slate: "border-slate-200 bg-slate-50 text-slate-700",
    cyan: "border-cyan-200 bg-cyan-50 text-cyan-700",
    violet: "border-violet-200 bg-violet-50 text-violet-700",
    amber: "border-amber-200 bg-amber-50 text-amber-700",
    emerald: "border-emerald-200 bg-emerald-50 text-emerald-700",
  }[tone];

  return (
    <Card className={`border p-5 shadow-sm ${toneClass}`}>
      <p className="text-xs font-bold uppercase tracking-[0.24em]">{label}</p>
      <p className="mt-3 text-3xl font-bold">{value}</p>
    </Card>
  );
}

function InfoTile({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-slate-100 bg-slate-50 p-4">
      <div className="text-xs font-bold uppercase tracking-[0.18em] text-slate-400">
        {label}
      </div>
      <div className="mt-2 text-sm font-medium text-slate-800">{value}</div>
    </div>
  );
}
