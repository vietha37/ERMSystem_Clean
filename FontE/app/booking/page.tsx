import { BookingForm } from "@/components/public/BookingForm";
import { PublicPageShell } from "@/components/public/PublicPageShell";
import { SectionHeading } from "@/components/public/SectionHeading";
import { hospitalCatalogService } from "@/services/hospitalCatalogService";

const fallbackServices = [
  "Kham tong quat cao cap",
  "Lay mau xet nghiem tai nha",
  "Kham chuyen khoa tim mach",
  "Kham san phu khoa",
  "Tam soat doanh nghiep",
];

export default async function BookingPage() {
  let serviceOptions = fallbackServices;

  try {
    const services = await hospitalCatalogService.getServices();
    if (services.length > 0) {
      serviceOptions = services.map((service) => service.name);
    }
  } catch {
    serviceOptions = fallbackServices;
  }

  return (
    <PublicPageShell>
      <section className="mx-auto max-w-7xl px-4 py-16 md:px-6 md:py-22">
        <div className="grid gap-10 lg:grid-cols-[0.92fr_1.08fr] lg:items-start">
          <div>
            <SectionHeading
              eyebrow="Dat lich thong minh"
              title="Trang booking phai cho cam giac dang lam viec voi mot dieu phoi vien y te rieng."
              description="Nguoi benh can thay ro cac lua chon, biet khi nao duoc lien he va cam thay an tam ve du lieu suc khoe cua minh."
            />

            <div className="mt-8 grid gap-4">
              {[
                "Xac nhan lich qua dien thoai trong thoi gian ngan.",
                "Uu tien tu van dung chuyen khoa truoc khi chot lich.",
                "Co the phoi hop kham tai vien, lay mau tai nha va tai kham theo goi.",
              ].map((item) => (
                <div key={item} className="rounded-[1.6rem] border border-slate-200 bg-white p-5 text-sm leading-7 text-slate-700 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
                  {item}
                </div>
              ))}
            </div>
          </div>

          <BookingForm serviceOptions={serviceOptions} />
        </div>
      </section>
    </PublicPageShell>
  );
}
