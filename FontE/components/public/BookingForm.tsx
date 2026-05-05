"use client";

import { FormEvent, useState } from "react";
import toast from "react-hot-toast";

export function BookingForm({
  serviceOptions,
}: {
  serviceOptions: string[];
}) {
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);

    await new Promise((resolve) => setTimeout(resolve, 800));
    toast.success("Da tiep nhan yeu cau. Dieu phoi vien se lien he xac nhan lich trong it phut.");
    setSubmitting(false);
    event.currentTarget.reset();
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="grid gap-4 rounded-[2rem] border border-slate-200 bg-white/92 p-6 shadow-[0_30px_90px_rgba(15,23,42,0.08)] backdrop-blur md:grid-cols-2 md:p-8"
    >
      <Field label="Ho va ten" placeholder="Nguyen Van A" />
      <Field label="So dien thoai" placeholder="09xx xxx xxx" />
      <Field label="Email" placeholder="tenban@email.com" />
      <label className="grid gap-2 text-sm font-medium text-slate-700">
        Dich vu quan tam
        <select className="h-12 rounded-2xl border border-slate-200 bg-slate-50 px-4 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white">
          {serviceOptions.map((service) => (
            <option key={service}>{service}</option>
          ))}
        </select>
      </label>
      <Field label="Ngay mong muon" type="date" />
      <Field label="Khung gio" placeholder="08:00 - 10:00" />
      <label className="md:col-span-2 grid gap-2 text-sm font-medium text-slate-700">
        Nhu cau cu the
        <textarea
          rows={4}
          placeholder="Vi du: can dat lich cho 2 nguoi, uu tien bac si nu, can ket qua xet nghiem trong ngay."
          className="rounded-3xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white"
        />
      </label>

      <div className="md:col-span-2 flex flex-col gap-4 pt-2 md:flex-row md:items-center md:justify-between">
        <p className="max-w-2xl text-sm leading-6 text-slate-500">
          Bang viec gui form, ban dong y de dieu phoi vien lien he tu van, xac nhan lich va ho tro chuan bi truoc kham.
        </p>
        <button
          type="submit"
          disabled={submitting}
          className="inline-flex h-12 items-center justify-center rounded-full bg-slate-950 px-6 text-sm font-semibold text-white transition hover:bg-cyan-700 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {submitting ? "Dang gui yeu cau..." : "Gui yeu cau dat lich"}
        </button>
      </div>
    </form>
  );
}

function Field({
  label,
  placeholder,
  type = "text",
}: {
  label: string;
  placeholder?: string;
  type?: string;
}) {
  return (
    <label className="grid gap-2 text-sm font-medium text-slate-700">
      {label}
      <input
        type={type}
        placeholder={placeholder}
        className="h-12 rounded-2xl border border-slate-200 bg-slate-50 px-4 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white"
      />
    </label>
  );
}
