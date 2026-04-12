import api from "./api";
import {
  CreateMedicalRecordPayload,
  MedicalRecord,
  PaginatedResult,
  UpdateMedicalRecordPayload,
} from "./types";

export const medicalRecordService = {
  getAll: async (
    pageNumber = 1,
    pageSize = 50
  ): Promise<PaginatedResult<MedicalRecord>> => {
    const response = await api.get<PaginatedResult<MedicalRecord>>(
      `/medicalrecords?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  },

  getById: async (id: string): Promise<MedicalRecord> => {
    const response = await api.get<MedicalRecord>(`/medicalrecords/${id}`);
    return response.data;
  },

  create: async (data: CreateMedicalRecordPayload): Promise<MedicalRecord> => {
    const response = await api.post<MedicalRecord>("/medicalrecords", data);
    return response.data;
  },

  update: async (
    id: string,
    data: UpdateMedicalRecordPayload
  ): Promise<void> => {
    await api.put(`/medicalrecords/${id}`, data);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/medicalrecords/${id}`);
  },
};
