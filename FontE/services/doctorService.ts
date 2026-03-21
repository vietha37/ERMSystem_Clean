import api from './api';

export const doctorService = {
  getAll: async (pageNumber = 1, pageSize = 100) => {
    const response = await api.get(`/doctors?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return response.data;
  },
  getById: async (id: string | number) => {
    const response = await api.get(`/doctors/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/doctors', data);
    return response.data;
  },
  update: async (id: string | number, data: any) => {
    const response = await api.put(`/doctors/${id}`, data);
    return response.data;
  },
  delete: async (id: string | number) => {
    const response = await api.delete(`/doctors/${id}`);
    return response.data;
  }
};
