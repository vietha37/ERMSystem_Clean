import { BookingForm } from "@/components/public/BookingForm";
import { PublicPageShell } from "@/components/public/PublicPageShell";
import { SectionHeading } from "@/components/public/SectionHeading";
import { hospitalCatalogService } from "@/services/hospitalCatalogService";
import { hospitalDoctorService } from "@/services/hospitalDoctorService";

const fallbackServices = [
  "Kham tong quat cao cap",
  "Lay mau xet nghiem tai nha",
  "Kham chuyen khoa tim mach",
  "Kham san phu khoa",
  "Tam soat doanh nghiep",
];

export default async function BookingPage() {
  let serviceOptions = fallbackServices;
  let specialties = [] as Awaited<ReturnType<typeof hospitalCatalogService.getSpecialties>>;
  let doctors = [] as Awaited<ReturnType<typeof hospitalDoctorService.getAll>>;

  try {
    const services = await hospitalCatalogService.getServices();
    if (services.length > 0) {
      serviceOptions = services.map((service) => `${service.serviceCode} - ${service.name}`);
    }
  } catch {
    serviceOptions = fallbackServices;
  }

  try {
    specialties = await hospitalCatalogService.getSpecialties();
  } catch {
    specialties = [];
  }

  try {
    doctors = await hospitalDoctorService.getAll();
  } catch {
    doctors = [];
  }

  return (
    <PublicPageShell>
      <section className="mx-auto max-w-7xl px-4 py-16 md:px-6 md:py-22">
        <div className="grid gap-10 lg:grid-cols-[0.92fr_1.08fr] lg:items-start">
          <div>
            <SectionHeading
              eyebrow="Dat lich thong minh"
              title="Trang booking da bat dau di vao luong dat lich that tren database dich."
              description="Nguoi benh chon chuyen khoa, bac si, ngay gio va gui yeu cau. He thong kiem tra lich lam viec va tao lich hen neu hop le."
            />

            <div className="mt-8 grid gap-4">
              {[
                "Kiem tra khung gio dua tren lich lam viec cua bac si.",
                "Tu dong tao ho so benh nhan moi neu chua ton tai trong he thong dich.",
                "Day su kien AppointmentCreated.v1 vao notification outbox de xu ly nhac lich o phase sau.",
              ].map((item) => (
                <div key={item} className="rounded-[1.6rem] border border-slate-200 bg-white p-5 text-sm leading-7 text-slate-700 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
                  {item}
                </div>
              ))}
            </div>
          </div>

          <BookingForm
            serviceOptions={serviceOptions}
            specialtyOptions={specialties.map((specialty) => ({
              id: specialty.id,
              name: specialty.name,
            }))}
            doctors={doctors}
          />
        </div>
      </section>
    </PublicPageShell>
  );
}
