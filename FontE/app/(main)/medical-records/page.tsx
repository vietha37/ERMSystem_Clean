"use client";

import React, { useEffect, useState } from "react";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Modal } from "@/components/ui/Modal";
import { medicalRecordService } from "@/services/medicalRecordService";
import { appointmentService } from "@/services/appointmentService";
import { patientService } from "@/services/patientService";
import { doctorService } from "@/services/doctorService";
import { getApiErrorMessage } from "@/services/error";
import { Appointment, Doctor, MedicalRecord, Patient } from "@/services/types";
import toast from "react-hot-toast";

function formatDateTime(value?: string): string {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return date.toLocaleString("vi-VN", {
    hour12: false,
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export default function MedicalRecordsPage() {
  const [records, setRecords] = useState<MedicalRecord[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<MedicalRecord | null>(null);

  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;
  const totalPages = Math.ceil((records?.length || 0) / pageSize);

  const [formData, setFormData] = useState({
    appointmentId: "",
    symptoms: "",
    diagnosis: "",
    notes: "",
  });

  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [recsDb, apptsDb, ptsDb, docsDb] = await Promise.all([
        medicalRecordService.getAll(1, 100),
        appointmentService.getAll(1, 100),
        patientService.getAll(1, 100),
        doctorService.getAll(1, 100),
      ]);

      setRecords(recsDb?.items || recsDb || []);
      setAppointments(apptsDb?.items || apptsDb || []);
      setPatients(ptsDb?.items || ptsDb || []);
      setDoctors(docsDb?.items || docsDb || []);
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Failed to load medical records data."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.appointmentId || !formData.diagnosis) {
      toast.error("Appointment and Diagnosis are required.");
      return;
    }

    setIsSubmitting(true);
    try {
      await medicalRecordService.create({ ...formData });
      toast.success("Medical record created successfully!");
      setIsModalOpen(false);
      setFormData({ appointmentId: "", symptoms: "", diagnosis: "", notes: "" });
      fetchData();
    } catch (error: unknown) {
      toast.error(getApiErrorMessage(error, "Failed to create medical record."));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this medical record? This cannot be undone.")) return;
    try {
      await medicalRecordService.delete(id);
      toast.success("Medical record deleted.");
      fetchData();
    } catch {
      toast.error("Failed to delete history.");
    }
  };

  const getPatientName = (id: string) => {
    const p = patients.find((item) => item.id === id);
    return p ? p.fullName : `Patient ${id}`;
  };

  const getDoctorName = (id: string) => {
    const d = doctors.find((item) => item.id === id);
    return d ? `Dr. ${d.fullName}` : `Doctor ${id}`;
  };

  const getAppointmentDetails = (appointmentId: string) =>
    appointments.find((a) => a.id === appointmentId);

  const selectedAppointmentData = formData.appointmentId
    ? getAppointmentDetails(formData.appointmentId)
    : null;

  const selectedRecordAppointment = selectedRecord
    ? getAppointmentDetails(selectedRecord.appointmentId)
    : null;

  const currentRecords = records.slice(
    (currentPage - 1) * pageSize,
    Math.min(currentPage * pageSize, records.length)
  );

  const openDetails = (record: MedicalRecord) => {
    setSelectedRecord(record);
    setIsDetailModalOpen(true);
  };

  const closeDetails = () => {
    setIsDetailModalOpen(false);
    setSelectedRecord(null);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Medical Records</h1>
          <p className="mt-1 text-sm text-gray-500">Manage patient diagnosis and consultation notes.</p>
        </div>
        <Button onClick={() => setIsModalOpen(true)}>+ Add Medical Record</Button>
      </div>

      <Card className="overflow-hidden border border-gray-100 p-0">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center p-16">
            <div className="mb-4 h-10 w-10 animate-spin rounded-full border-b-2 border-blue-600" />
            <p className="text-sm font-medium text-gray-500">Loading medical records...</p>
          </div>
        ) : (
          <div className="flex min-h-[500px] flex-col">
            <div className="flex-1 overflow-x-auto">
              <table className="min-w-max w-full border-collapse text-left">
                <thead>
                  <tr className="border-b border-gray-200 bg-blue-50">
                    <th className="p-4 text-sm font-semibold text-gray-700">Patient Name</th>
                    <th className="p-4 text-sm font-semibold text-gray-700">Doctor Name</th>
                    <th className="p-4 text-sm font-semibold text-gray-700">Appointment Date</th>
                    <th className="p-4 text-sm font-semibold text-gray-700">Diagnosis</th>
                    <th className="p-4 text-sm font-semibold text-gray-700">Created At</th>
                    <th className="p-4 text-right text-sm font-semibold text-gray-700">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {currentRecords.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="p-10 text-center text-sm text-gray-500">
                        No medical records found.
                      </td>
                    </tr>
                  ) : (
                    currentRecords.map((rec) => {
                      const appt = getAppointmentDetails(rec.appointmentId);
                      return (
                        <tr key={rec.id} className="border-b border-gray-100 transition-colors hover:bg-blue-50/60">
                          <td className="p-4 font-medium text-gray-800">
                            {appt ? getPatientName(appt.patientId) : "N/A"}
                          </td>
                          <td className="p-4 text-gray-600">{appt ? getDoctorName(appt.doctorId) : "N/A"}</td>
                          <td className="p-4 text-sm text-gray-600">
                            {appt ? formatDateTime(appt.appointmentDate) : "N/A"}
                          </td>
                          <td className="max-w-[220px] truncate p-4 font-medium text-blue-600">{rec.diagnosis}</td>
                          <td className="p-4 text-sm text-gray-500">
                            {formatDateTime(rec.createdAt ?? appt?.appointmentDate)}
                          </td>
                          <td className="flex justify-end gap-2 p-4">
                            <button
                              className="rounded-lg border border-blue-100 px-3 py-1.5 text-xs font-medium text-blue-600 transition-colors hover:bg-blue-50"
                              onClick={() => openDetails(rec)}
                            >
                              View Details
                            </button>
                            <button
                              onClick={() => handleDelete(rec.id)}
                              className="rounded-lg border border-red-100 px-3 py-1.5 text-xs font-medium text-red-500 transition-colors hover:bg-red-50"
                            >
                              Delete
                            </button>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>

            {totalPages > 0 && (
              <div className="flex items-center justify-between border-t border-gray-100 bg-white p-4">
                <span className="text-sm text-gray-600">
                  Showing {(currentPage - 1) * pageSize + 1} to{" "}
                  {Math.min(currentPage * pageSize, records.length)} of {records.length} entries
                </span>
                <div className="flex gap-1">
                  <button
                    disabled={currentPage === 1}
                    onClick={() => setCurrentPage(1)}
                    className="rounded-lg border border-gray-200 px-3 py-1 text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50"
                  >
                    « First
                  </button>
                  <button
                    disabled={currentPage === 1}
                    onClick={() => setCurrentPage((p) => p - 1)}
                    className="rounded-lg border border-gray-200 px-3 py-1 text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50"
                  >
                    ‹ Prev
                  </button>
                  <button
                    disabled={currentPage === totalPages}
                    onClick={() => setCurrentPage((p) => p + 1)}
                    className="rounded-lg border border-gray-200 px-3 py-1 text-sm font-medium text-blue-600 hover:bg-blue-50 disabled:opacity-50"
                  >
                    Next ›
                  </button>
                </div>
              </div>
            )}
          </div>
        )}
      </Card>

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Create Medical Record">
        <form onSubmit={handleCreate} className="mt-2 space-y-5">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Link to Appointment</label>
            <select
              className="w-full cursor-pointer rounded-lg border border-gray-300 bg-white px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              value={formData.appointmentId}
              onChange={(e) => setFormData({ ...formData, appointmentId: e.target.value })}
              required
            >
              <option value="">-- Choose Completed Appointment --</option>
              {appointments
                .filter((a) => a.status === "Completed")
                .map((a) => (
                  <option key={a.id} value={a.id}>
                    {new Date(a.appointmentDate).toLocaleDateString()} - {getPatientName(a.patientId)}
                  </option>
                ))}
            </select>
          </div>

          {selectedAppointmentData && (
            <div className="flex flex-col gap-1 rounded-xl border border-blue-100 bg-blue-50/70 p-4 text-sm shadow-sm">
              <p>
                <span className="font-semibold text-gray-700">Patient:</span>{" "}
                {getPatientName(selectedAppointmentData.patientId)}
              </p>
              <p>
                <span className="font-semibold text-gray-700">Doctor:</span>{" "}
                {getDoctorName(selectedAppointmentData.doctorId)}
              </p>
              <p>
                <span className="font-semibold text-gray-700">Date:</span>{" "}
                {formatDateTime(selectedAppointmentData.appointmentDate)}
              </p>
            </div>
          )}

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Diagnosis</label>
            <input
              type="text"
              className="w-full rounded-lg border border-gray-300 px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              placeholder="e.g Acute Bronchitis"
              value={formData.diagnosis}
              onChange={(e) => setFormData({ ...formData, diagnosis: e.target.value })}
              required
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Symptoms</label>
            <textarea
              required
              rows={3}
              className="w-full resize-y rounded-lg border border-gray-300 px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              placeholder="Describe the patient's symptoms..."
              value={formData.symptoms}
              onChange={(e) => setFormData({ ...formData, symptoms: e.target.value })}
            />
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Treatment Notes (Optional)</label>
            <textarea
              rows={4}
              className="w-full resize-y rounded-lg border border-gray-300 px-4 py-2 outline-none transition-all focus:border-blue-400 focus:ring-2 focus:ring-blue-400"
              placeholder="Record any treatment notes or prescriptions given here..."
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
            />
          </div>

          <div className="flex justify-end gap-3 border-t border-gray-100 pt-4">
            <Button type="button" variant="secondary" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Saving..." : "Save Record"}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal isOpen={isDetailModalOpen} onClose={closeDetails} title="Medical Record Details">
        {selectedRecord ? (
          <div className="space-y-4 text-sm">
            <div className="grid grid-cols-1 gap-3 rounded-xl border border-gray-100 bg-gray-50 p-4 md:grid-cols-2">
              <div>
                <p className="text-gray-500">Patient</p>
                <p className="font-semibold text-gray-800">
                  {selectedRecordAppointment ? getPatientName(selectedRecordAppointment.patientId) : "N/A"}
                </p>
              </div>
              <div>
                <p className="text-gray-500">Doctor</p>
                <p className="font-semibold text-gray-800">
                  {selectedRecordAppointment ? getDoctorName(selectedRecordAppointment.doctorId) : "N/A"}
                </p>
              </div>
              <div>
                <p className="text-gray-500">Appointment Date</p>
                <p className="font-semibold text-gray-800">
                  {selectedRecordAppointment ? formatDateTime(selectedRecordAppointment.appointmentDate) : "N/A"}
                </p>
              </div>
              <div>
                <p className="text-gray-500">Created At</p>
                <p className="font-semibold text-gray-800">
                  {formatDateTime(selectedRecord.createdAt ?? selectedRecordAppointment?.appointmentDate)}
                </p>
              </div>
            </div>

            <div className="rounded-xl border border-gray-100 p-4">
              <p className="mb-1 text-gray-500">Diagnosis</p>
              <p className="font-semibold text-blue-700">{selectedRecord.diagnosis || "N/A"}</p>
            </div>

            <div className="rounded-xl border border-gray-100 p-4">
              <p className="mb-1 text-gray-500">Symptoms</p>
              <p className="whitespace-pre-wrap text-gray-800">{selectedRecord.symptoms || "N/A"}</p>
            </div>

            <div className="rounded-xl border border-gray-100 p-4">
              <p className="mb-1 text-gray-500">Treatment Notes</p>
              <p className="whitespace-pre-wrap text-gray-800">{selectedRecord.notes || "N/A"}</p>
            </div>

            <div className="flex justify-end pt-2">
              <Button type="button" variant="secondary" onClick={closeDetails}>
                Close
              </Button>
            </div>
          </div>
        ) : (
          <div className="py-6 text-center text-gray-500">No detail available.</div>
        )}
      </Modal>
    </div>
  );
}
