"use client";

import React, { useEffect, useState } from "react";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Modal } from "@/components/ui/Modal";
import { appointmentService } from "@/services/appointmentService";
import { patientService } from "@/services/patientService";
import { doctorService } from "@/services/doctorService";
import { getApiErrorMessage } from "@/services/error";
import {
  Appointment,
  AppointmentPayload,
  AppointmentStatus,
  Doctor,
  Patient,
} from "@/services/types";
import toast from "react-hot-toast";

const STATUS_COLORS: Record<AppointmentStatus, string> = {
  Pending: "bg-yellow-500 text-white",
  Completed: "bg-green-500 text-white",
  Cancelled: "bg-red-500 text-white",
};

const EMPTY_FORM: AppointmentPayload = {
  patientId: "",
  doctorId: "",
  appointmentDate: "",
  status: "Pending",
};

export default function AppointmentsPage() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState<AppointmentPayload>(EMPTY_FORM);

  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [apptsDb, ptsDb, docsDb] = await Promise.all([
        appointmentService.getAll(1, 100),
        patientService.getAll(1, 100),
        doctorService.getAll(1, 100),
      ]);

      setAppointments(apptsDb.items);
      setPatients(ptsDb.items);
      setDoctors(docsDb.items);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Failed to load data."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.patientId || !formData.doctorId || !formData.appointmentDate) {
      toast.error("Please fill in all required fields.");
      return;
    }

    setIsSubmitting(true);
    try {
      await appointmentService.create(formData);
      toast.success("Appointment created successfully!");
      setIsModalOpen(false);
      setFormData(EMPTY_FORM);
      fetchData();
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Failed to create appointment."));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this appointment?")) {
      return;
    }

    try {
      await appointmentService.delete(id);
      toast.success("Appointment deleted.");
      fetchData();
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Failed to delete appointment."));
    }
  };

  const handleStatusUpdate = async (
    id: string,
    newStatus: AppointmentStatus
  ) => {
    try {
      const current = appointments.find((appointment) => appointment.id === id);
      if (!current) {
        toast.error("Appointment not found.");
        return;
      }

      await appointmentService.update(id, {
        patientId: current.patientId,
        doctorId: current.doctorId,
        appointmentDate: current.appointmentDate,
        status: newStatus,
      });

      toast.success(`Status updated to ${newStatus}.`);
      fetchData();
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Failed to update status."));
    }
  };

  const getPatientName = (id: string): string => {
    const patient = patients.find((entry) => entry.id === id);
    return patient ? patient.fullName : `Unknown ID: ${id}`;
  };

  const getDoctorName = (id: string): string => {
    const doctor = doctors.find((entry) => entry.id === id);
    return doctor ? `Dr. ${doctor.fullName}` : `Unknown ID: ${id}`;
  };

  return (
    <div className="animate-fade-in space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Appointments</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage scheduled visits and medical consultations.
          </p>
        </div>
        <Button onClick={() => setIsModalOpen(true)}>+ New Appointment</Button>
      </div>

      <Card className="overflow-hidden border border-gray-100 p-0">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center p-16">
            <div className="mb-4 h-10 w-10 animate-spin rounded-full border-b-2 border-blue-600" />
            <p className="text-sm font-medium text-gray-500">
              Loading appointments...
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-max w-full border-collapse text-left">
              <thead>
                <tr className="border-b border-gray-200 bg-blue-50">
                  <th className="p-4 text-sm font-semibold text-gray-700">
                    Patient
                  </th>
                  <th className="p-4 text-sm font-semibold text-gray-700">
                    Doctor
                  </th>
                  <th className="p-4 text-sm font-semibold text-gray-700">
                    Appointment Date
                  </th>
                  <th className="p-4 text-sm font-semibold text-gray-700">
                    Status
                  </th>
                  <th className="p-4 text-sm font-semibold text-gray-700">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {appointments.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="p-10 text-center text-sm text-gray-500">
                      No appointments found.
                    </td>
                  </tr>
                ) : (
                  appointments.map((appointment) => (
                    <tr
                      key={appointment.id}
                      className="border-b border-gray-100 transition-colors hover:bg-blue-50/60"
                    >
                      <td className="p-4 font-medium text-gray-800">
                        {getPatientName(appointment.patientId)}
                      </td>
                      <td className="p-4 text-gray-600">
                        {getDoctorName(appointment.doctorId)}
                      </td>
                      <td className="p-4 font-mono text-sm text-gray-600">
                        {new Date(appointment.appointmentDate).toLocaleString()}
                      </td>
                      <td className="p-4">
                        <span
                          className={`rounded-full px-3 py-1 text-xs font-semibold shadow-sm ${STATUS_COLORS[appointment.status]}`}
                        >
                          {appointment.status}
                        </span>
                      </td>
                      <td className="flex items-center gap-3 p-4">
                        <select
                          className="block cursor-pointer rounded-lg border border-gray-300 bg-white px-2 py-1.5 text-xs text-gray-700 outline-none focus:border-blue-500 focus:ring-blue-500"
                          value={appointment.status}
                          onChange={(e) =>
                            handleStatusUpdate(
                              appointment.id,
                              e.target.value as AppointmentStatus
                            )
                          }
                        >
                          <option value="Pending">Pending</option>
                          <option value="Completed">Completed</option>
                          <option value="Cancelled">Cancelled</option>
                        </select>
                        <button
                          onClick={() => handleDelete(appointment.id)}
                          className="rounded-lg border border-red-200 px-3 py-1.5 text-xs font-medium text-red-500 transition-colors hover:bg-red-500 hover:text-white"
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Create New Appointment"
      >
        <form onSubmit={handleCreate} className="mt-2 space-y-5">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Select Patient
            </label>
            <select
              className="w-full cursor-pointer rounded-lg border border-gray-300 bg-white px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              value={formData.patientId}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, patientId: e.target.value }))
              }
              required
            >
              <option value="">-- Choose Patient --</option>
              {patients.map((patient) => (
                <option key={patient.id} value={patient.id}>
                  {patient.fullName}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Select Doctor
            </label>
            <select
              className="w-full cursor-pointer rounded-lg border border-gray-300 bg-white px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              value={formData.doctorId}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, doctorId: e.target.value }))
              }
              required
            >
              <option value="">-- Choose Doctor --</option>
              {doctors.map((doctor) => (
                <option key={doctor.id} value={doctor.id}>
                  Dr. {doctor.fullName}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Appointment Date &amp; Time
            </label>
            <input
              type="datetime-local"
              className="w-full rounded-lg border border-gray-300 px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              value={formData.appointmentDate}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  appointmentDate: e.target.value,
                }))
              }
              required
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Status
            </label>
            <select
              className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              value={formData.status}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  status: e.target.value as AppointmentStatus,
                }))
              }
            >
              <option value="Pending">Pending</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>

          <div className="flex justify-end gap-3 border-t border-gray-100 pt-5">
            <Button
              type="button"
              variant="secondary"
              onClick={() => setIsModalOpen(false)}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Saving..." : "Save Appointment"}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
