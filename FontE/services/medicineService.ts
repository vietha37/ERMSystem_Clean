import api from "./api";
import {
  CreateMedicinePayload,
  Medicine,
  PaginatedResult,
  UpdateMedicinePayload,
} from "./types";

export const medicineService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 100
  ): Promise<PaginatedResult<Medicine>> => {
    const response = await api.get<PaginatedResult<Medicine>>(
      `/medicines?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  },

  getById: async (id: string): Promise<Medicine> => {
    const response = await api.get<Medicine>(`/medicines/${id}`);
    return response.data;
  },

  create: async (data: CreateMedicinePayload): Promise<Medicine> => {
    const response = await api.post<Medicine>("/medicines", data);
    return response.data;
  },

  update: async (id: string, data: UpdateMedicinePayload): Promise<void> => {
    await api.put(`/medicines/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/medicines/${id}`);
  },
};
