import { PublicPageShell } from "@/components/public/PublicPageShell";
import { SectionHeading } from "@/components/public/SectionHeading";
import { doctors } from "@/content/hospitalContent";

export default function DoctorsPage() {
  return (
    <PublicPageShell>
      <section className="mx-auto max-w-7xl px-4 py-16 md:px-6 md:py-22">
        <SectionHeading
          eyebrow="Đội ngũ bác sĩ"
          title="Hồ sơ bác sĩ cần đủ chiều sâu để tạo niềm tin ngay từ lần xem đầu tiên."
          description="Một website bệnh viện tư mạnh không giấu chuyên môn. Nó cho thấy rõ bác sĩ điều trị nhóm bệnh nào, có bao nhiêu kinh nghiệm và người bệnh nên đặt lịch trong bối cảnh nào."
        />

        <div className="mt-12 grid gap-5 lg:grid-cols-2">
          {doctors.map((doctor) => (
            <article key={doctor.name} className="rounded-[2rem] border border-slate-200 bg-white p-7 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
              <div className="flex flex-wrap items-start justify-between gap-4">
                <div>
                  <p className="text-sm uppercase tracking-[0.22em] text-cyan-700">{doctor.specialty}</p>
                  <h2 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950">{doctor.name}</h2>
                  <p className="mt-2 text-sm text-slate-500">{doctor.title}</p>
                </div>
                <div className="rounded-full bg-slate-100 px-4 py-2 text-sm font-medium text-slate-700">{doctor.experience}</div>
              </div>

              <div className="mt-6 flex flex-wrap gap-2">
                {doctor.focus.map((item) => (
                  <span key={item} className="rounded-full border border-slate-200 px-3 py-2 text-sm text-slate-700">
                    {item}
                  </span>
                ))}
              </div>
            </article>
          ))}
        </div>
      </section>
    </PublicPageShell>
  );
}
