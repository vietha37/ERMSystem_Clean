import api from "./api";
import {
  AddPrescriptionItemPayload,
  CreatePrescriptionPayload,
  PaginatedResult,
  Prescription,
} from "./types";

export const prescriptionService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 50
  ): Promise<PaginatedResult<Prescription>> => {
    const response = await api.get<PaginatedResult<Prescription>>(
      `/prescriptions?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  },

  getById: async (id: string): Promise<Prescription> => {
    const response = await api.get<Prescription>(`/prescriptions/${id}`);
    return response.data;
  },

  create: async (data: CreatePrescriptionPayload): Promise<Prescription> => {
    const response = await api.post<Prescription>("/prescriptions", data);
    return response.data;
  },

  addMedicine: async (
    prescriptionId: string,
    data: AddPrescriptionItemPayload
  ): Promise<Prescription> => {
    const response = await api.post<Prescription>(
      `/prescriptions/${prescriptionId}/items`,
      data
    );
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/prescriptions/${id}`);
  },
};
