import api from './api';

export const appointmentService = {
  getAll: async (pageNumber = 1, pageSize = 50) => {
    const response = await api.get(`/appointments?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return response.data;
  },
  getById: async (id: string | number) => {
    const response = await api.get(`/appointments/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/appointments', data);
    return response.data;
  },
  updateStatus: async (id: string | number, status: string) => {
    const response = await api.put(`/appointments/${id}`, { status });
    return response.data;
  },
  delete: async (id: string | number) => {
    const response = await api.delete(`/appointments/${id}`);
    return response.data;
  }
};
