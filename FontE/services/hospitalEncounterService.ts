import api from "./api";
import {
  CreateHospitalEncounterPayload,
  HospitalEncounterDetail,
  HospitalEncounterEligibleAppointment,
  HospitalEncounterSummary,
  HospitalEncounterWorklistQuery,
  PaginatedResult,
  UpdateHospitalEncounterPayload,
} from "./types";

export const hospitalEncounterService = {
  getAll: async (
    query: HospitalEncounterWorklistQuery = {}
  ): Promise<PaginatedResult<HospitalEncounterSummary>> => {
    const params = new URLSearchParams();
    params.set("pageNumber", String(query.pageNumber ?? 1));
    params.set("pageSize", String(query.pageSize ?? 10));

    if (query.encounterStatus && query.encounterStatus !== "All") {
      params.set("encounterStatus", query.encounterStatus);
    }

    if (query.appointmentDate) {
      params.set("appointmentDate", query.appointmentDate);
    }

    if (query.textSearch?.trim()) {
      params.set("textSearch", query.textSearch.trim());
    }

    const response = await api.get<PaginatedResult<HospitalEncounterSummary>>(
      `/hospital-encounters?${params.toString()}`
    );

    return response.data;
  },

  getById: async (encounterId: string): Promise<HospitalEncounterDetail> => {
    const response = await api.get<HospitalEncounterDetail>(
      `/hospital-encounters/${encounterId}`
    );

    return response.data;
  },

  getEligibleAppointments: async (): Promise<HospitalEncounterEligibleAppointment[]> => {
    const response = await api.get<HospitalEncounterEligibleAppointment[]>(
      "/hospital-encounters/eligible-appointments"
    );

    return response.data;
  },

  create: async (
    payload: CreateHospitalEncounterPayload
  ): Promise<HospitalEncounterDetail> => {
    const response = await api.post<HospitalEncounterDetail>(
      "/hospital-encounters",
      payload
    );

    return response.data;
  },

  update: async (
    encounterId: string,
    payload: UpdateHospitalEncounterPayload
  ): Promise<HospitalEncounterDetail> => {
    const response = await api.put<HospitalEncounterDetail>(
      `/hospital-encounters/${encounterId}`,
      payload
    );

    return response.data;
  },
};
