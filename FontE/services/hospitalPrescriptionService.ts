import api from "./api";
import {
  CreateHospitalPrescriptionPayload,
  HospitalMedicineCatalog,
  HospitalPrescriptionDetail,
  HospitalPrescriptionEligibleEncounter,
  HospitalPrescriptionSummary,
  HospitalPrescriptionWorklistQuery,
  PaginatedResult,
} from "./types";

export const hospitalPrescriptionService = {
  getAll: async (
    query: HospitalPrescriptionWorklistQuery = {}
  ): Promise<PaginatedResult<HospitalPrescriptionSummary>> => {
    const params = new URLSearchParams();
    params.set("pageNumber", String(query.pageNumber ?? 1));
    params.set("pageSize", String(query.pageSize ?? 10));

    if (query.status && query.status !== "All") {
      params.set("status", query.status);
    }

    if (query.textSearch?.trim()) {
      params.set("textSearch", query.textSearch.trim());
    }

    const response = await api.get<PaginatedResult<HospitalPrescriptionSummary>>(
      `/hospital-prescriptions?${params.toString()}`
    );

    return response.data;
  },

  getById: async (prescriptionId: string): Promise<HospitalPrescriptionDetail> => {
    const response = await api.get<HospitalPrescriptionDetail>(
      `/hospital-prescriptions/${prescriptionId}`
    );

    return response.data;
  },

  getEligibleEncounters: async (): Promise<HospitalPrescriptionEligibleEncounter[]> => {
    const response = await api.get<HospitalPrescriptionEligibleEncounter[]>(
      "/hospital-prescriptions/eligible-encounters"
    );

    return response.data;
  },

  getMedicineCatalog: async (): Promise<HospitalMedicineCatalog[]> => {
    const response = await api.get<HospitalMedicineCatalog[]>(
      "/hospital-prescriptions/medicine-catalog"
    );

    return response.data;
  },

  create: async (
    payload: CreateHospitalPrescriptionPayload
  ): Promise<HospitalPrescriptionDetail> => {
    const response = await api.post<HospitalPrescriptionDetail>(
      "/hospital-prescriptions",
      payload
    );

    return response.data;
  },

  delete: async (prescriptionId: string): Promise<void> => {
    await api.delete(`/hospital-prescriptions/${prescriptionId}`);
  },
};
