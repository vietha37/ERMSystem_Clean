import api from "./api";
import { Appointment, AppointmentPayload, PaginatedResult } from "./types";

export const appointmentService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 50
  ): Promise<PaginatedResult<Appointment>> => {
    const response = await api.get<PaginatedResult<Appointment>>(
      `/appointments?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  },

  getById: async (id: string): Promise<Appointment> => {
    const response = await api.get<Appointment>(`/appointments/${id}`);
    return response.data;
  },

  create: async (data: AppointmentPayload): Promise<Appointment> => {
    const response = await api.post<Appointment>("/appointments", data);
    return response.data;
  },

  update: async (id: string, data: AppointmentPayload): Promise<void> => {
    await api.put(`/appointments/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/appointments/${id}`);
  },
};
