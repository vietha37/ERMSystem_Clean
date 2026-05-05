export type PublicHospitalAppointmentBookingPayload = {
  fullName: string;
  phone: string;
  email?: string;
  dateOfBirth: string;
  gender: string;
  doctorProfileId: string;
  specialtyId?: string;
  serviceCode?: string;
  preferredDate: string;
  preferredTime: string;
  chiefComplaint?: string;
  notes?: string;
};

export type PublicHospitalAppointmentBookingResult = {
  appointmentId: string;
  appointmentNumber: string;
  patientId: string;
  doctorProfileId: string;
  doctorName: string;
  specialtyName: string;
  clinicName: string;
  appointmentStartLocal: string;
  appointmentEndLocal: string;
  status: string;
  isExistingPatient: boolean;
  notificationQueued: boolean;
};

function getApiBaseUrl() {
  return process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5219/api";
}

export const hospitalAppointmentService = {
  async bookPublicAppointment(
    payload: PublicHospitalAppointmentBookingPayload
  ): Promise<PublicHospitalAppointmentBookingResult> {
    const response = await fetch(`${getApiBaseUrl()}/hospital-appointments/public-booking`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      let message = "Khong the dat lich luc nay.";
      try {
        const data = (await response.json()) as { message?: string };
        if (data?.message) {
          message = data.message;
        }
      } catch {
        // Ignore JSON parse errors and return fallback message.
      }

      throw new Error(message);
    }

    return response.json() as Promise<PublicHospitalAppointmentBookingResult>;
  },
};
