import api from './api';

export const medicineService = {
  getAll: async (pageNumber = 1, pageSize = 100) => {
    const response = await api.get(`/medicines?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return response.data;
  },
  getById: async (id: string | number) => {
    const response = await api.get(`/medicines/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/medicines', data);
    return response.data;
  },
  update: async (id: string | number, data: any) => {
    const response = await api.put(`/medicines/${id}`, data);
    return response.data;
  },
  delete: async (id: string | number) => {
    const response = await api.delete(`/medicines/${id}`);
    return response.data;
  }
};
