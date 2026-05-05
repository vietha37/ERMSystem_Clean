export type HospitalDepartment = {
  id: string;
  departmentCode: string;
  name: string;
  description?: string | null;
  isActive: boolean;
};

export type HospitalSpecialty = {
  id: string;
  specialtyCode: string;
  name: string;
  departmentId?: string | null;
  departmentName?: string | null;
  isActive: boolean;
};

export type HospitalClinic = {
  id: string;
  clinicCode: string;
  name: string;
  departmentId?: string | null;
  departmentName?: string | null;
  floorLabel?: string | null;
  roomLabel?: string | null;
  isActive: boolean;
};

export type HospitalServiceCatalog = {
  id: string;
  serviceCode: string;
  name: string;
  category: string;
  unitPrice: number;
  isActive: boolean;
};

export type HospitalCatalogOverview = {
  departments: HospitalDepartment[];
  specialties: HospitalSpecialty[];
  clinics: HospitalClinic[];
  services: HospitalServiceCatalog[];
};

function getApiBaseUrl() {
  return process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5219/api";
}

async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`Khong the tai du lieu tu ${path}`);
  }

  return response.json() as Promise<T>;
}

export const hospitalCatalogService = {
  getOverview: () => getJson<HospitalCatalogOverview>("/hospital-catalog/overview"),
  getDepartments: () => getJson<HospitalDepartment[]>("/hospital-catalog/departments"),
  getSpecialties: () => getJson<HospitalSpecialty[]>("/hospital-catalog/specialties"),
  getClinics: () => getJson<HospitalClinic[]>("/hospital-catalog/clinics"),
  getServices: () => getJson<HospitalServiceCatalog[]>("/hospital-catalog/services"),
};
