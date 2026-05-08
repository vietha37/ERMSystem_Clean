import api from "./api";
import {
  CreateHospitalClinicalOrderPayload,
  HospitalClinicalOrderCatalogItem,
  HospitalClinicalOrderDetail,
  HospitalClinicalOrderEligibleEncounter,
  HospitalClinicalOrderSummary,
  HospitalClinicalOrderWorklistQuery,
  PaginatedResult,
  RecordHospitalImagingReportPayload,
  RecordHospitalLabResultPayload,
} from "./types";

export const hospitalClinicalOrderService = {
  getAll: async (
    query: HospitalClinicalOrderWorklistQuery = {}
  ): Promise<PaginatedResult<HospitalClinicalOrderSummary>> => {
    const params = new URLSearchParams();
    params.set("pageNumber", String(query.pageNumber ?? 1));
    params.set("pageSize", String(query.pageSize ?? 10));

    if (query.category && query.category !== "All") {
      params.set("category", query.category);
    }

    if (query.status && query.status !== "All") {
      params.set("status", query.status);
    }

    if (query.textSearch?.trim()) {
      params.set("textSearch", query.textSearch.trim());
    }

    const response = await api.get<PaginatedResult<HospitalClinicalOrderSummary>>(
      `/hospital-clinical-orders?${params.toString()}`
    );

    return response.data;
  },

  getById: async (clinicalOrderId: string): Promise<HospitalClinicalOrderDetail> => {
    const response = await api.get<HospitalClinicalOrderDetail>(
      `/hospital-clinical-orders/${clinicalOrderId}`
    );

    return response.data;
  },

  getEligibleEncounters: async (): Promise<HospitalClinicalOrderEligibleEncounter[]> => {
    const response = await api.get<HospitalClinicalOrderEligibleEncounter[]>(
      "/hospital-clinical-orders/eligible-encounters"
    );

    return response.data;
  },

  getCatalog: async (): Promise<HospitalClinicalOrderCatalogItem[]> => {
    const response = await api.get<HospitalClinicalOrderCatalogItem[]>(
      "/hospital-clinical-orders/catalog"
    );

    return response.data;
  },

  create: async (
    payload: CreateHospitalClinicalOrderPayload
  ): Promise<HospitalClinicalOrderDetail> => {
    const response = await api.post<HospitalClinicalOrderDetail>(
      "/hospital-clinical-orders",
      payload
    );

    return response.data;
  },

  recordLabResult: async (
    clinicalOrderId: string,
    payload: RecordHospitalLabResultPayload
  ): Promise<HospitalClinicalOrderDetail> => {
    const response = await api.post<HospitalClinicalOrderDetail>(
      `/hospital-clinical-orders/${clinicalOrderId}/lab-result`,
      payload
    );

    return response.data;
  },

  recordImagingReport: async (
    clinicalOrderId: string,
    payload: RecordHospitalImagingReportPayload
  ): Promise<HospitalClinicalOrderDetail> => {
    const response = await api.post<HospitalClinicalOrderDetail>(
      `/hospital-clinical-orders/${clinicalOrderId}/imaging-report`,
      payload
    );

    return response.data;
  },
};
