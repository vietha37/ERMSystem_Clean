import api from "./api";
import { TodayNotifications } from "./types";

export const notificationService = {
  getToday: async (): Promise<TodayNotifications> => {
    const response = await api.get<TodayNotifications>("/notifications/today");
    return response.data;
  },
};
