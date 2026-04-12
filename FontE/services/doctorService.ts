import api from "./api";
import {
  CreateDoctorPayload,
  Doctor,
  PaginatedResult,
  UpdateDoctorPayload,
} from "./types";

export const doctorService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 100
  ): Promise<PaginatedResult<Doctor>> => {
    const response = await api.get<PaginatedResult<Doctor>>(
      `/doctors?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  },

  getById: async (id: string): Promise<Doctor> => {
    const response = await api.get<Doctor>(`/doctors/${id}`);
    return response.data;
  },

  create: async (data: CreateDoctorPayload): Promise<Doctor> => {
    const response = await api.post<Doctor>("/doctors", data);
    return response.data;
  },

  update: async (id: string, data: UpdateDoctorPayload): Promise<void> => {
    await api.put(`/doctors/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/doctors/${id}`);
  },
};
