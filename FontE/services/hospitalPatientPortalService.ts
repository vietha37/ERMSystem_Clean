import api from "./api";
import { HospitalPatientPortalOverview } from "./types";

export const hospitalPatientPortalService = {
  getMyOverview: async (): Promise<HospitalPatientPortalOverview> => {
    const response = await api.get<HospitalPatientPortalOverview>(
      "/hospital-patient-portal/me"
    );

    return response.data;
  },
};
