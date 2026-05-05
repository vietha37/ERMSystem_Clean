"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import {
  hospitalAppointmentService,
  type PublicHospitalAppointmentBookingPayload,
} from "@/services/hospitalAppointmentService";
import { type HospitalDoctor } from "@/services/hospitalDoctorService";

export function BookingForm({
  serviceOptions,
  specialtyOptions,
  doctors,
}: {
  serviceOptions: string[];
  specialtyOptions: Array<{ id: string; name: string }>;
  doctors: HospitalDoctor[];
}) {
  const [submitting, setSubmitting] = useState(false);
  const [selectedSpecialtyId, setSelectedSpecialtyId] = useState(specialtyOptions[0]?.id ?? "");
  const [selectedDoctorId, setSelectedDoctorId] = useState("");

  const filteredDoctors = useMemo(() => {
    if (!selectedSpecialtyId) {
      return doctors;
    }

    return doctors.filter((doctor) => doctor.specialtyId === selectedSpecialtyId);
  }, [doctors, selectedSpecialtyId]);

  const selectedDoctor = filteredDoctors.find((doctor) => doctor.doctorProfileId === selectedDoctorId) ?? filteredDoctors[0];

  useEffect(() => {
    if (!filteredDoctors.length) {
      setSelectedDoctorId("");
      return;
    }

    if (!filteredDoctors.some((doctor) => doctor.doctorProfileId === selectedDoctorId)) {
      setSelectedDoctorId(filteredDoctors[0].doctorProfileId);
    }
  }, [filteredDoctors, selectedDoctorId]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedDoctor) {
      toast.error("He thong chua co bac si phu hop de dat lich.");
      return;
    }

    setSubmitting(true);

    const form = event.currentTarget;
    const formData = new FormData(form);

    const payload: PublicHospitalAppointmentBookingPayload = {
      fullName: String(formData.get("fullName") ?? "").trim(),
      phone: String(formData.get("phone") ?? "").trim(),
      email: String(formData.get("email") ?? "").trim() || undefined,
      dateOfBirth: String(formData.get("dateOfBirth") ?? ""),
      gender: String(formData.get("gender") ?? ""),
      doctorProfileId: selectedDoctor.doctorProfileId,
      specialtyId: selectedSpecialtyId || undefined,
      serviceCode: String(formData.get("serviceCode") ?? "").trim() || undefined,
      preferredDate: String(formData.get("preferredDate") ?? ""),
      preferredTime: String(formData.get("preferredTime") ?? ""),
      chiefComplaint: String(formData.get("chiefComplaint") ?? "").trim() || undefined,
      notes: String(formData.get("notes") ?? "").trim() || undefined,
    };

    try {
      const result = await hospitalAppointmentService.bookPublicAppointment(payload);
      toast.success(`Da dat lich thanh cong. Ma lich hen: ${result.appointmentNumber}`);
      form.reset();
      setSelectedSpecialtyId(specialtyOptions[0]?.id ?? "");
      setSelectedDoctorId("");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Khong the dat lich luc nay.";
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="grid gap-4 rounded-[2rem] border border-slate-200 bg-white/92 p-6 shadow-[0_30px_90px_rgba(15,23,42,0.08)] backdrop-blur md:grid-cols-2 md:p-8"
    >
      <Field name="fullName" label="Ho va ten" placeholder="Nguyen Van A" required />
      <Field name="phone" label="So dien thoai" placeholder="09xx xxx xxx" required />
      <Field name="email" label="Email" placeholder="tenban@email.com" />
      <Field name="dateOfBirth" label="Ngay sinh" type="date" required />
      <SelectField
        label="Gioi tinh"
        name="gender"
        options={[
          { value: "Nam", label: "Nam" },
          { value: "Nu", label: "Nu" },
          { value: "Khac", label: "Khac" },
        ]}
      />
      <SelectField
        label="Chuyen khoa"
        name="specialtyId"
        value={selectedSpecialtyId}
        onChange={setSelectedSpecialtyId}
        options={specialtyOptions.map((specialty) => ({
          value: specialty.id,
          label: specialty.name,
        }))}
      />
      <SelectField
        label="Bac si"
        name="doctorProfileId"
        value={selectedDoctorId}
        onChange={setSelectedDoctorId}
        disabled={!filteredDoctors.length}
        options={filteredDoctors.map((doctor) => ({
          value: doctor.doctorProfileId,
          label: `${doctor.fullName} - ${doctor.specialtyName}`,
        }))}
      />
      <label className="grid gap-2 text-sm font-medium text-slate-700">
        Dich vu quan tam
        <select
          name="serviceCode"
          className="h-12 rounded-2xl border border-slate-200 bg-slate-50 px-4 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white"
        >
          {serviceOptions.map((service) => (
            <option key={service}>{service}</option>
          ))}
        </select>
      </label>
      <Field name="preferredDate" label="Ngay mong muon" type="date" required />
      <Field name="preferredTime" label="Gio bat dau" type="time" required />
      <label className="md:col-span-2 grid gap-2 text-sm font-medium text-slate-700">
        Trieu chung / nhu cau chinh
        <textarea
          name="chiefComplaint"
          rows={4}
          placeholder="Vi du: dau nguc nhe, can tu van tim mach, muon dat lich buoi sang."
          className="rounded-3xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white"
        />
      </label>
      <label className="md:col-span-2 grid gap-2 text-sm font-medium text-slate-700">
        Ghi chu bo sung
        <textarea
          name="notes"
          rows={3}
          placeholder="Thong tin them cho dieu phoi vien neu can."
          className="rounded-3xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white"
        />
      </label>

      {selectedDoctor ? (
        <div className="md:col-span-2 rounded-[1.6rem] border border-cyan-100 bg-cyan-50/60 p-4 text-sm text-slate-700">
          <p className="font-semibold text-slate-900">Lich lam viec hien co cua bac si {selectedDoctor.fullName}</p>
          <div className="mt-3 flex flex-wrap gap-2">
            {selectedDoctor.schedules.map((schedule) => (
              <span key={schedule.scheduleId} className="rounded-full border border-cyan-200 bg-white px-3 py-2 text-xs text-slate-700">
                {formatDayOfWeek(schedule.dayOfWeek)} {schedule.startTime.slice(0, 5)}-{schedule.endTime.slice(0, 5)} | {schedule.clinicName}
              </span>
            ))}
          </div>
        </div>
      ) : null}

      <div className="md:col-span-2 flex flex-col gap-4 pt-2 md:flex-row md:items-center md:justify-between">
        <p className="max-w-2xl text-sm leading-6 text-slate-500">
          Sau khi gui, he thong se tao lich hen va dua su kien vao outbox de Notification Service co the nhac lich o cac buoc sau.
        </p>
        <button
          type="submit"
          disabled={submitting || !selectedDoctor}
          className="inline-flex h-12 items-center justify-center rounded-full bg-slate-950 px-6 text-sm font-semibold text-white transition hover:bg-cyan-700 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {submitting ? "Dang gui yeu cau..." : "Gui yeu cau dat lich"}
        </button>
      </div>
    </form>
  );
}

function Field({
  name,
  label,
  placeholder,
  type = "text",
  required = false,
}: {
  name: string;
  label: string;
  placeholder?: string;
  type?: string;
  required?: boolean;
}) {
  return (
    <label className="grid gap-2 text-sm font-medium text-slate-700">
      {label}
      <input
        name={name}
        type={type}
        placeholder={placeholder}
        required={required}
        className="h-12 rounded-2xl border border-slate-200 bg-slate-50 px-4 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white"
      />
    </label>
  );
}

function SelectField({
  label,
  name,
  options,
  value,
  onChange,
  disabled = false,
}: {
  label: string;
  name: string;
  options: Array<{ value: string; label: string }>;
  value?: string;
  onChange?: (value: string) => void;
  disabled?: boolean;
}) {
  return (
    <label className="grid gap-2 text-sm font-medium text-slate-700">
      {label}
      <select
        name={name}
        value={value}
        disabled={disabled}
        onChange={(event) => onChange?.(event.target.value)}
        className="h-12 rounded-2xl border border-slate-200 bg-slate-50 px-4 text-sm text-slate-900 outline-none transition focus:border-cyan-500 focus:bg-white disabled:cursor-not-allowed disabled:opacity-70"
      >
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  );
}

function formatDayOfWeek(dayOfWeek: number) {
  switch (dayOfWeek) {
    case 1:
      return "Thu 2";
    case 2:
      return "Thu 3";
    case 3:
      return "Thu 4";
    case 4:
      return "Thu 5";
    case 5:
      return "Thu 6";
    case 6:
      return "Thu 7";
    case 0:
      return "Chu nhat";
    default:
      return "Khac";
  }
}
