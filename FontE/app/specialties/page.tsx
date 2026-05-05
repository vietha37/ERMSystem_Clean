import { PublicPageShell } from "@/components/public/PublicPageShell";
import { SectionHeading } from "@/components/public/SectionHeading";
import { specialties } from "@/content/hospitalContent";
import { hospitalCatalogService } from "@/services/hospitalCatalogService";

export default async function SpecialtiesPage() {
  let specialtiesFromApi = [] as Awaited<ReturnType<typeof hospitalCatalogService.getSpecialties>>;

  try {
    specialtiesFromApi = await hospitalCatalogService.getSpecialties();
  } catch {
    specialtiesFromApi = [];
  }

  const hasApiData = specialtiesFromApi.length > 0;

  return (
    <PublicPageShell>
      <section className="mx-auto max-w-7xl px-4 py-16 md:px-6 md:py-22">
        <SectionHeading
          eyebrow="He chuyen khoa"
          title="Chuyen khoa duoc dong bo theo danh muc nghiep vu thay vi chi dung noi dung gioi thieu."
          description="Day la diem neo de dat lich, dieu phoi va phan quyen van hanh noi bo bam cung mot cau truc du lieu."
        />

        {hasApiData ? (
          <div className="mt-12 grid gap-5 lg:grid-cols-2 xl:grid-cols-3">
            {specialtiesFromApi.map((specialty) => (
              <article key={specialty.id} className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
                <p className="text-sm uppercase tracking-[0.22em] text-cyan-700">
                  {specialty.departmentName ?? "He chuyen khoa"}
                </p>
                <h2 className="mt-4 text-3xl font-semibold tracking-tight text-slate-950">{specialty.name}</h2>
                <p className="mt-4 text-sm font-medium leading-7 text-slate-700">Ma chuyen khoa: {specialty.specialtyCode}</p>
                <p className="mt-4 text-sm leading-7 text-slate-600">
                  Chuyen khoa nay dang duoc doc tu database dich de frontend, dat lich va dashboard noi bo su dung cung mot danh muc.
                </p>
              </article>
            ))}
          </div>
        ) : (
          <div className="mt-12 grid gap-5 lg:grid-cols-2 xl:grid-cols-3">
            {specialties.map((specialty) => (
              <article key={specialty.title} className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
                <p className="text-sm uppercase tracking-[0.22em] text-cyan-700">Specialty</p>
                <h2 className="mt-4 text-3xl font-semibold tracking-tight text-slate-950">{specialty.title}</h2>
                <p className="mt-4 text-sm font-medium leading-7 text-slate-700">{specialty.lead}</p>
                <p className="mt-4 text-sm leading-7 text-slate-600">{specialty.description}</p>
              </article>
            ))}
          </div>
        )}
      </section>
    </PublicPageShell>
  );
}
