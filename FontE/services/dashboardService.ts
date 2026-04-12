import api from './api';
import { DashboardStats, DashboardTrends } from "./types";

type TrendsParams = {
  period: "daily" | "monthly";
  fromDate?: string;
  toDate?: string;
};

export const dashboardService = {
  getStats: async (): Promise<DashboardStats> => {
    const response = await api.get<DashboardStats>('/dashboard/stats');
    return response.data;
  },

  getTrends: async ({ period, fromDate, toDate }: TrendsParams): Promise<DashboardTrends> => {
    const params = new URLSearchParams({ period });
    if (fromDate) params.set("fromDate", fromDate);
    if (toDate) params.set("toDate", toDate);

    const response = await api.get<DashboardTrends>(`/dashboard/trends?${params.toString()}`);
    return response.data;
  }
};
