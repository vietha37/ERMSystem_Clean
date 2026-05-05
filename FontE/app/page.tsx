import Link from "next/link";
import { PublicPageShell } from "@/components/public/PublicPageShell";
import { SectionHeading } from "@/components/public/SectionHeading";
import {
  doctors,
  hospitalStats,
  newsArticles,
  patientJourney,
  quickActions,
  serviceCategories,
  specialties,
} from "@/content/hospitalContent";

export default function HomePage() {
  return (
    <PublicPageShell>
      <section className="relative overflow-hidden">
        <div className="mx-auto grid max-w-7xl gap-12 px-4 py-16 md:px-6 lg:grid-cols-[1.15fr_0.85fr] lg:items-center lg:py-24">
          <div className="animate-rise">
            <p className="text-sm font-semibold uppercase tracking-[0.32em] text-cyan-700">
              Hệ sinh thái chăm sóc sức khỏe tư nhân
            </p>
            <h1 className="mt-5 max-w-4xl font-serif text-5xl leading-none tracking-tight text-slate-950 md:text-7xl">
              Bệnh viện tư thiết kế để người bệnh thấy an tâm từ trước khi bước vào quầy tiếp đón.
            </h1>
            <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-600">
              ERM Private Hospital kết hợp khám đa khoa, cận lâm sàng, bác sĩ chuyên gia và chăm sóc sau khám
              trên cùng một hành trình số hóa. Mọi điểm chạm từ đặt lịch, tiếp đón đến trả kết quả đều được tổ
              chức như một dịch vụ concierge y tế.
            </p>

            <div className="mt-8 flex flex-wrap gap-4">
              <Link
                href="/booking"
                className="inline-flex h-12 items-center justify-center rounded-full bg-slate-950 px-6 text-sm font-semibold text-white transition hover:bg-cyan-700"
              >
                Đặt lịch ưu tiên
              </Link>
              <Link
                href="/services"
                className="inline-flex h-12 items-center justify-center rounded-full border border-slate-300 bg-white px-6 text-sm font-semibold text-slate-900 transition hover:border-cyan-600 hover:text-cyan-700"
              >
                Xem danh mục dịch vụ
              </Link>
            </div>

            <div className="mt-12 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              {hospitalStats.map((item) => (
                <div key={item.label} className="rounded-[1.8rem] border border-white/70 bg-white/78 p-5 shadow-[0_18px_45px_rgba(15,23,42,0.05)] backdrop-blur">
                  <p className="text-3xl font-semibold tracking-tight text-slate-950">{item.value}</p>
                  <p className="mt-2 text-sm leading-6 text-slate-600">{item.label}</p>
                </div>
              ))}
            </div>
          </div>

          <div className="relative">
            <div className="absolute -left-12 top-12 h-32 w-32 rounded-full bg-cyan-300/30 blur-3xl" />
            <div className="absolute -right-10 bottom-10 h-36 w-36 rounded-full bg-emerald-300/25 blur-3xl" />
            <div className="relative overflow-hidden rounded-[2rem] border border-slate-200 bg-slate-950 p-8 text-white shadow-[0_35px_90px_rgba(15,23,42,0.22)]">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm uppercase tracking-[0.24em] text-cyan-200">Concierge Medical Desk</p>
                  <p className="mt-3 max-w-xs text-3xl font-semibold leading-tight">
                    Một đầu mối điều phối toàn bộ hành trình khám của gia đình.
                  </p>
                </div>
                <div className="rounded-full border border-white/15 px-4 py-2 text-sm text-cyan-100">Ưu tiên riêng tư</div>
              </div>

              <div className="mt-8 grid gap-4">
                {[
                  "Đặt lịch theo bác sĩ hoặc chuyên khoa chỉ với một yêu cầu duy nhất.",
                  "Gợi ý gói khám, chuẩn bị trước khám và nhắc lịch tái khám tự động.",
                  "Liên thông kết quả xét nghiệm, chẩn đoán hình ảnh và đơn thuốc trong cùng hồ sơ.",
                ].map((item) => (
                  <div key={item} className="rounded-[1.5rem] border border-white/10 bg-white/6 p-4 text-sm leading-6 text-slate-200">
                    {item}
                  </div>
                ))}
              </div>

              <div className="mt-8 grid gap-3 rounded-[1.5rem] bg-white px-5 py-5 text-slate-900">
                <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-700">Liên hệ nhanh</p>
                <div className="grid gap-2 text-sm text-slate-600">
                  <p>Hotline ưu tiên: 1900 565 656</p>
                  <p>Email concierge: concierge@ermhospital.vn</p>
                  <p>Hỗ trợ lấy mẫu tại nhà và đặt lịch doanh nghiệp toàn quốc.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-8 md:px-6 md:py-14">
        <SectionHeading
          eyebrow="Tiện ích cho khách hàng"
          title="Những thao tác quan trọng nhất luôn ở lớp đầu của website."
          description="Học từ mô hình bệnh viện tư thực tế, homepage cần ưu tiên các tác vụ có ý định rõ: đặt lịch, lấy mẫu tại nhà, tra cứu dịch vụ và kết nối bác sĩ."
        />
        <div className="mt-10 grid gap-5 md:grid-cols-2 xl:grid-cols-4">
          {quickActions.map((action) => (
            <Link
              key={action.title}
              href={action.href}
              className="group overflow-hidden rounded-[2rem] border border-slate-200 bg-white shadow-[0_20px_55px_rgba(15,23,42,0.06)] transition hover:-translate-y-1 hover:shadow-[0_24px_70px_rgba(8,145,178,0.14)]"
            >
              <div className={`h-2 bg-gradient-to-r ${action.accent}`} />
              <div className="p-6">
                <h3 className="text-2xl font-semibold tracking-tight text-slate-950">{action.title}</h3>
                <p className="mt-3 text-sm leading-7 text-slate-600">{action.description}</p>
                <span className="mt-8 inline-flex text-sm font-semibold text-cyan-700 transition group-hover:translate-x-1">
                  Khám phá →
                </span>
              </div>
            </Link>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-8 md:px-6 md:py-14">
        <SectionHeading
          eyebrow="Dịch vụ mũi nhọn"
          title="Thiết kế danh mục dịch vụ theo nhu cầu thực tế của một bệnh viện tư."
          description="Các khối dịch vụ cần rõ giá trị, đầu ra và nhóm khách hàng phù hợp, thay vì chỉ liệt kê tên khoa."
        />
        <div className="mt-10 grid gap-5 lg:grid-cols-2">
          {serviceCategories.map((service) => (
            <article key={service.title} className="rounded-[2rem] border border-slate-200 bg-white p-7 shadow-[0_20px_50px_rgba(15,23,42,0.05)]">
              <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-700">Service line</p>
              <h3 className="mt-4 text-3xl font-semibold tracking-tight text-slate-950">{service.title}</h3>
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
      </section>

      <section className="mx-auto max-w-7xl px-4 py-8 md:px-6 md:py-14">
        <SectionHeading
          eyebrow="Hệ chuyên khoa"
          title="Cấu trúc chuyên khoa cần rõ vai trò để người bệnh tự định hướng đúng cửa vào."
          description="Một website bệnh viện tư tốt sẽ giải thích chuyên khoa bằng bài toán sức khỏe cụ thể, không để khách hàng phải tự đoán mình nên bắt đầu ở đâu."
        />
        <div className="mt-10 grid gap-5 lg:grid-cols-3">
          {specialties.map((specialty) => (
            <article key={specialty.title} className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
              <p className="text-sm uppercase tracking-[0.22em] text-cyan-700">Specialty</p>
              <h3 className="mt-4 text-3xl font-semibold tracking-tight text-slate-950">{specialty.title}</h3>
              <p className="mt-4 text-sm font-medium leading-7 text-slate-700">{specialty.lead}</p>
              <p className="mt-4 text-sm leading-7 text-slate-600">{specialty.description}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-8 md:px-6 md:py-14">
        <div className="grid gap-8 rounded-[2.5rem] bg-slate-950 px-6 py-10 text-white md:px-10 lg:grid-cols-[0.9fr_1.1fr]">
          <SectionHeading
            eyebrow="Lộ trình người bệnh"
            title="Một private hospital website cần kể được cả quy trình chứ không chỉ bán dịch vụ."
            description="Từ phía marketing đến sản phẩm, hành trình công khai phải khớp với vận hành thật bên trong bệnh viện."
          />
          <div className="grid gap-4">
            {patientJourney.map((step, index) => (
              <div key={step} className="rounded-[1.6rem] border border-white/10 bg-white/5 p-5">
                <p className="text-xs uppercase tracking-[0.28em] text-cyan-200">Bước {index + 1}</p>
                <p className="mt-3 text-base leading-7 text-slate-100">{step}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 py-8 md:px-6 md:py-14">
        <SectionHeading
          eyebrow="Đội ngũ chuyên gia"
          title="Bác sĩ là lớp niềm tin quan trọng nhất của một website bệnh viện tư."
          description="Website cần trình bày rõ chuyên khoa, kinh nghiệm, trọng tâm điều trị và khả năng đặt lịch ngay với từng bác sĩ."
        />
        <div className="mt-10 grid gap-5 lg:grid-cols-2">
          {doctors.map((doctor) => (
            <article key={doctor.name} className="rounded-[2rem] border border-slate-200 bg-white p-7 shadow-[0_20px_55px_rgba(15,23,42,0.05)]">
              <div className="flex flex-wrap items-center justify-between gap-4">
                <div>
                  <p className="text-sm uppercase tracking-[0.2em] text-cyan-700">{doctor.specialty}</p>
                  <h3 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950">{doctor.name}</h3>
                  <p className="mt-2 text-sm text-slate-500">{doctor.title}</p>
                </div>
                <span className="rounded-full bg-cyan-50 px-4 py-2 text-sm font-medium text-cyan-800">{doctor.experience}</span>
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

      <section className="mx-auto max-w-7xl px-4 py-8 md:px-6 md:py-14">
        <SectionHeading
          eyebrow="Tin tức và kiến thức"
          title="Lớp nội dung chuyên môn giúp website không chỉ là brochure."
          description="Tin sức khỏe, khuyến nghị chuyên gia và cẩm nang theo dõi tại nhà là phần bắt buộc nếu muốn website có chiều sâu như một hệ thống bệnh viện tư thực tế."
        />
        <div className="mt-10 grid gap-5 lg:grid-cols-3">
          {newsArticles.map((article) => (
            <article key={article.title} className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-[0_20px_50px_rgba(15,23,42,0.05)]">
              <p className="text-sm uppercase tracking-[0.22em] text-cyan-700">{article.category}</p>
              <h3 className="mt-4 text-2xl font-semibold tracking-tight text-slate-950">{article.title}</h3>
              <p className="mt-4 text-sm leading-7 text-slate-600">{article.summary}</p>
              <p className="mt-6 text-sm font-medium text-slate-500">{article.readTime}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 pb-8 pt-8 md:px-6 md:pb-14 md:pt-14">
        <div className="rounded-[2.5rem] border border-cyan-200 bg-gradient-to-br from-cyan-50 via-white to-emerald-50 p-8 shadow-[0_22px_60px_rgba(8,145,178,0.08)] md:p-10">
          <SectionHeading
            eyebrow="Mở rộng hệ thống"
            title="Từ website public này, mình có thể nâng tiếp toàn bộ mặt trước của dự án."
            description="Lớp tiếp theo hợp lý là làm đồng bộ các trang dashboard nội bộ, module đặt lịch thật, tra cứu kết quả, hồ sơ bác sĩ chi tiết và tin tức động từ CMS/API."
          />
          <div className="mt-8 flex flex-wrap gap-4">
            <Link
              href="/specialties"
              className="inline-flex h-12 items-center justify-center rounded-full bg-slate-950 px-6 text-sm font-semibold text-white transition hover:bg-cyan-700"
            >
              Xem toàn bộ chuyên khoa
            </Link>
            <Link
              href="/doctors"
              className="inline-flex h-12 items-center justify-center rounded-full border border-slate-300 bg-white px-6 text-sm font-semibold text-slate-900 transition hover:border-cyan-600 hover:text-cyan-700"
            >
              Khám phá đội ngũ bác sĩ
            </Link>
          </div>
        </div>
      </section>
    </PublicPageShell>
  );
}
