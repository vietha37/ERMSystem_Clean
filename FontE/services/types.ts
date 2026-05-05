export type Id = string;

export type AppointmentStatus = "Pending" | "Completed" | "Cancelled";
export type UserRole = "Admin" | "Doctor" | "Receptionist" | "Patient";

export type PaginatedResult<T> = {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export type Patient = {
  id: Id;
  fullName: string;
  dateOfBirth: string;
  gender: string;
  phone: string;
  address: string;
  createdAt: string;
};

export type Doctor = {
  id: Id;
  fullName: string;
  specialty: string;
};

export type Appointment = {
  id: Id;
  patientId: Id;
  doctorId: Id;
  appointmentDate: string;
  status: AppointmentStatus;
};

export type MedicalRecord = {
  id: Id;
  appointmentId: Id;
  symptoms: string;
  diagnosis: string;
  notes: string;
  createdAt?: string;
};

export type Medicine = {
  id: Id;
  name: string;
  description: string;
};

export type CreateMedicinePayload = {
  name: string;
  description: string;
};

export type UpdateMedicinePayload = CreateMedicinePayload;

export type PrescriptionItem = {
  id: Id;
  prescriptionId: Id;
  medicineId: Id;
  dosage: string;
  duration: string;
};

export type Prescription = {
  id: Id;
  medicalRecordId: Id;
  createdAt: string;
  items: PrescriptionItem[];
};

export type DashboardStats = {
  totalPatients: number;
  appointmentsToday: number;
  completedAppointments: number;
  topDiagnoses: Record<string, number>;
};

export type DashboardTrendPoint = {
  label: string;
  patientsCount: number;
  prescriptionsCount: number;
};

export type DashboardTrends = {
  period: "daily" | "monthly";
  fromDate: string;
  toDate: string;
  currentPatientsTotal: number;
  currentPrescriptionsTotal: number;
  previousPatientsTotal: number;
  previousPrescriptionsTotal: number;
  points: DashboardTrendPoint[];
};

export type AppointmentNotification = {
  appointmentId: string;
  appointmentDate: string;
  patientName: string;
  doctorName: string;
  status: string;
  message: string;
};

export type TodayNotifications = {
  unreadCount: number;
  notifications: AppointmentNotification[];
};

export type NotificationDeliveryStatus = "Queued" | "Delivered" | "Failed" | "Skipped";

export type NotificationDelivery = {
  id: Id;
  outboxMessageId: Id;
  channelCode: string;
  recipient: string;
  deliveryStatus: NotificationDeliveryStatus;
  providerMessageId?: string | null;
  attemptCount: number;
  lastAttemptAtUtc?: string | null;
  deliveredAtUtc?: string | null;
  errorMessage?: string | null;
};

export type NotificationDeliveryListResult = {
  totalCount: number;
  items: NotificationDelivery[];
};

export type HospitalPatientPortalProfile = {
  patientId: Id;
  medicalRecordNumber: string;
  fullName: string;
  dateOfBirth: string;
  gender: string;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  portalStatus: string;
  activatedAtUtc: string;
};

export type HospitalPatientPortalAppointment = {
  appointmentId: Id;
  appointmentNumber: string;
  status: string;
  appointmentType: string;
  bookingChannel: string;
  appointmentStartLocal: string;
  appointmentEndLocal?: string | null;
  doctorName: string;
  specialtyName: string;
  clinicName: string;
  chiefComplaint?: string | null;
};

export type HospitalPatientPortalOverview = {
  profile: HospitalPatientPortalProfile;
  upcomingAppointments: HospitalPatientPortalAppointment[];
  recentAppointments: HospitalPatientPortalAppointment[];
};

export type AuthResponse = {
  token: string;
  accessToken: string;
  refreshToken: string;
  username: string;
  role: string;
  expiresAt: string;
};

export type CreatePatientPayload = {
  fullName: string;
  dateOfBirth: string;
  gender: string;
  phone: string;
  address: string;
};

export type PatientRegisterPayload = CreatePatientPayload & {
  username: string;
  password: string;
};

export type UpdatePatientPayload = CreatePatientPayload;

export type CreateDoctorPayload = {
  fullName: string;
  specialty: string;
};

export type UpdateDoctorPayload = CreateDoctorPayload;

export type StaffUser = {
  id: Id;
  username: string;
  role: Exclude<UserRole, "Admin" | "Patient">;
};

export type CreateStaffUserPayload = {
  username: string;
  password: string;
  role: Exclude<UserRole, "Admin" | "Patient">;
};

export type UpdateStaffUserPayload = {
  username: string;
  role: Exclude<UserRole, "Admin" | "Patient">;
  password?: string;
};

export type AppointmentPayload = {
  patientId: Id;
  doctorId: Id;
  appointmentDate: string;
  status: AppointmentStatus;
};

export type CreateMedicalRecordPayload = {
  appointmentId: Id;
  symptoms: string;
  diagnosis: string;
  notes: string;
};

export type UpdateMedicalRecordPayload = CreateMedicalRecordPayload;

export type CreatePrescriptionPayload = {
  medicalRecordId: Id;
};

export type AddPrescriptionItemPayload = {
  medicineId: Id;
  dosage: string;
  duration: string;
};
