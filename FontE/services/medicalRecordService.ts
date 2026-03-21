import api from './api';

export const medicalRecordService = {
  getAll: async (pageNumber = 1, pageSize = 50) => {
    const response = await api.get(`/medicalrecords?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return response.data;
  },
  getById: async (id: string | number) => {
    const response = await api.get(`/medicalrecords/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/medicalrecords', data);
    return response.data;
  },
  update: async (id: string | number, data: any) => {
    const response = await api.put(`/medicalrecords/${id}`, data);
    return response.data;
  },
  delete: async (id: string | number) => {
    const response = await api.delete(`/medicalrecords/${id}`);
    return response.data;
  }
};
