import api from './api';

export const patientService = {
  getAll: async (pageNumber = 1, pageSize = 10, search = '') => {
    let url = `/patients?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    const response = await api.get(url);
    return response.data;
  },
  getById: async (id: string | number) => {
    const response = await api.get(`/patients/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/patients', data);
    return response.data;
  },
  update: async (id: string | number, data: any) => {
    const response = await api.put(`/patients/${id}`, data);
    return response.data;
  },
  delete: async (id: string | number) => {
    const response = await api.delete(`/patients/${id}`);
    return response.data;
  }
};
