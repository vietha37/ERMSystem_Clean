import api from './api';

export const prescriptionService = {
  getAll: async (pageNumber = 1, pageSize = 50) => {
    const response = await api.get(`/prescriptions?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return response.data;
  },
  getById: async (id: string | number) => {
    const response = await api.get(`/prescriptions/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/prescriptions', data);
    return response.data;
  },
  addMedicine: async (data: { prescriptionId: number | string, medicineId: number | string, dosage: string, duration: string }) => {
    const { prescriptionId, ...payload } = data;
    const response = await api.post(`/prescriptions/${prescriptionId}/items`, payload);
    return response.data;
  },
  delete: async (id: string | number) => {
    const response = await api.delete(`/prescriptions/${id}`);
    return response.data;
  }
};
