import { PublicPageShell } from "@/components/public/PublicPageShell";
import { SectionHeading } from "@/components/public/SectionHeading";
import { serviceCategories } from "@/content/hospitalContent";
import { hospitalCatalogService } from "@/services/hospitalCatalogService";

function formatCurrency(amount: number) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount);
}

export default async function ServicesPage() {
  let servicesFromApi = [] as Awaited<ReturnType<typeof hospitalCatalogService.getServices>>;

  try {
    servicesFromApi = await hospitalCatalogService.getServices();
  } catch {
    servicesFromApi = [];
  }

  const groupedServices = servicesFromApi.reduce<Record<string, typeof servicesFromApi>>((acc, service) => {
    const key = service.category || "Khac";
    acc[key] ??= [];
    acc[key].push(service);
    return acc;
  }, {});

  const hasApiData = servicesFromApi.length > 0;

  return (
    <PublicPageShell>
      <section className="mx-auto max-w-7xl px-4 py-16 md:px-6 md:py-22">
        <SectionHeading
          eyebrow="Danh muc dich vu"
          title="Cau truc dich vu duoc dong bo voi database dich cua benh vien tu."
          description="Frontend co the bam truc tiep vao danh muc van hanh that thay vi chi dung noi dung mo ta co dinh."
        />

        {hasApiData ? (
          <div className="mt-12 grid gap-6 lg:grid-cols-2">
            {Object.entries(groupedServices).map(([category, items]) => (
              <article key={category} className="rounded-[2rem] border border-slate-200 bg-white p-7 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
                <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-700">{category}</p>
                <h2 className="mt-4 text-3xl font-semibold tracking-tight text-slate-950">
                  {items.length} dich vu san sang cho van hanh thuc te
                </h2>
                <p className="mt-4 text-sm leading-7 text-slate-600">
                  Danh muc nay duoc lay truc tiep tu ERMSystemHospitalDb de backend va frontend dung cung mot nguon su that.
                </p>
                <ul className="mt-6 grid gap-3">
                  {items.map((item) => (
                    <li key={item.id} className="rounded-2xl bg-slate-50 px-4 py-3 text-sm text-slate-700">
                      <div className="font-semibold text-slate-900">{item.name}</div>
                      <div className="mt-1 flex items-center justify-between gap-3 text-xs uppercase tracking-[0.18em] text-slate-500">
                        <span>{item.serviceCode}</span>
                        <span>{formatCurrency(item.unitPrice)}</span>
                      </div>
                    </li>
                  ))}
                </ul>
              </article>
            ))}
          </div>
        ) : (
          <div className="mt-12 grid gap-6 lg:grid-cols-2">
            {serviceCategories.map((service) => (
              <article key={service.title} className="rounded-[2rem] border border-slate-200 bg-white p-7 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
                <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-700">Healthcare program</p>
                <h2 className="mt-4 text-3xl font-semibold tracking-tight text-slate-950">{service.title}</h2>
                <p className="mt-4 text-sm leading-7 text-slate-600">{service.summary}</p>
                <ul className="mt-6 grid gap-3">
                  {service.items.map((item) => (
                    <li key={item} className="rounded-2xl bg-slate-50 px-4 py-3 text-sm text-slate-700">
                      {item}
                    </li>
                  ))}
                </ul>
              </article>
            ))}
          </div>
        )}
      </section>
    </PublicPageShell>
  );
}
