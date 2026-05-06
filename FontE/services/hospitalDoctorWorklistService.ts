import api from "./api";
import { HospitalDoctorWorklistResponse } from "./types";

export const hospitalDoctorWorklistService = {
  get: async (
    workDate?: string,
    doctorProfileId?: string
  ): Promise<HospitalDoctorWorklistResponse> => {
    const params = new URLSearchParams();

    if (workDate) {
      params.set("workDate", workDate);
    }

    if (doctorProfileId) {
      params.set("doctorProfileId", doctorProfileId);
    }

    const query = params.toString();
    const response = await api.get<HospitalDoctorWorklistResponse>(
      `/hospital-doctor-worklist${query ? `?${query}` : ""}`
    );

    return response.data;
  },
};
