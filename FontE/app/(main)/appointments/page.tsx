"use client";

import React, { useState, useEffect } from 'react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { appointmentService } from '@/services/appointmentService';
import { patientService } from '@/services/patientService';
import { doctorService } from '@/services/doctorService';
import toast from 'react-hot-toast';

export default function AppointmentsPage() {
  const [appointments, setAppointments] = useState<any[]>([]);
  const [patients, setPatients] = useState<any[]>([]);
  const [doctors, setDoctors] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    patientId: '',
    doctorId: '',
    appointmentDate: '',
    status: 'Pending'
  });

  const fetchData = async () => {
    setIsLoading(true);
    try {
      const apptsDb = await appointmentService.getAll(1, 100).catch(() => ({ items: [] }));
      const ptsDb = await patientService.getAll(1, 100).catch(() => ({ items: [] }));
      const docsDb = await doctorService.getAll(1, 100).catch(() => ({ items: [] }));

      const appts = apptsDb?.items || apptsDb || [];
      const pts = ptsDb?.items || ptsDb || [];
      const docs = docsDb?.items || docsDb || [];

      // Populate some local mock data matching UI requirements if backend is unreachable or empty
      if (!appts.length && !pts.length && !docs.length) {
         setPatients([{ id: 1, fullName: 'John Doe', firstName: 'John', lastName: 'Doe' }]);
         setDoctors([{ id: 1, fullName: 'Alice Smith', firstName: 'Alice', lastName: 'Smith' }]);
         setAppointments([{ id: 101, patientId: 1, doctorId: 1, appointmentDate: new Date().toISOString(), status: 'Pending' }]);
      } else {
         setPatients(pts);
         setDoctors(docs);
         setAppointments(appts);
      }
    } catch (error) {
      toast.error("Failed to load data.");
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
      toast.error('Please fill in all required fields.');
      return;
    }
    
    setIsSubmitting(true);
    try {
      await appointmentService.create(formData);
      toast.success('Appointment created successfully!');
      setIsModalOpen(false);
      fetchData();
      setFormData({ patientId: '', doctorId: '', appointmentDate: '', status: 'Pending' });
    } catch (error: any) {
      if (error.code === 'ERR_NETWORK') {
         setAppointments(prev => [...prev, { id: Math.floor(Math.random() * 10000), ...formData }]);
         toast.success('Appointment created (mock).');
         setIsModalOpen(false);
      } else {
         const msg = error.response?.data?.message || 'Failed to create appointment.';
         toast.error(msg);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this appointment?')) return;
    try {
      await appointmentService.delete(id);
      toast.success('Appointment deleted.');
      fetchData();
    } catch (error: any) {
      if (error.code === 'ERR_NETWORK') {
         setAppointments(prev => prev.filter(app => app.id !== id));
         toast.success('Appointment deleted (mock).');
      } else {
         toast.error('Failed to delete appointment.');
      }
    }
  };

  const handleStatusUpdate = async (id: number, newStatus: string) => {
    try {
      // Create copy of appointment strictly for mock fallback
      const oldAppt = appointments.find(a => a.id === id);
      await appointmentService.updateStatus(id, newStatus);
      toast.success(`Status updated to ${newStatus}.`);
      fetchData();
    } catch (error: any) {
      if (error.code === 'ERR_NETWORK') {
         setAppointments(prev => prev.map(app => app.id === id ? { ...app, status: newStatus } : app));
         toast.success(`Status updated to ${newStatus} (mock).`);
      } else {
         toast.error('Failed to update status.');
      }
    }
  };

  const getPatientName = (id: any) => {
    const p = patients.find(p => p.id == id);
    return p ? (p.fullName || `${p.firstName} ${p.lastName}`) : `Unknown ID: ${id}`;
  };

  const getDoctorName = (id: any) => {
    const d = doctors.find(d => d.id == id);
    return d ? (d.fullName ? `Dr. ${d.fullName}` : `Dr. ${d.firstName} ${d.lastName}`) : `Unknown ID: ${id}`;
  };

  const statusColors: any = {
    'Pending': 'bg-yellow-500 text-white',
    'Completed': 'bg-green-500 text-white',
    'Cancelled': 'bg-red-500 text-white',
  };

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex justify-between items-center">
        <div>
           <h1 className="text-2xl font-bold text-gray-800">Appointments</h1>
           <p className="text-gray-500 text-sm mt-1">Manage scheduled visits and medical consultations.</p>
        </div>
        <Button onClick={() => setIsModalOpen(true)}>+ New Appointment</Button>
      </div>

      <Card className="p-0 overflow-hidden border border-gray-100">
        {isLoading ? (
          <div className="p-16 flex flex-col justify-center items-center">
             <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mb-4"></div>
             <p className="text-gray-500 text-sm font-medium">Loading appointments...</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse min-w-max">
              <thead>
                <tr className="bg-blue-50 border-b border-gray-200">
                  <th className="p-4 text-sm font-semibold text-gray-700">Patient</th>
                  <th className="p-4 text-sm font-semibold text-gray-700">Doctor</th>
                  <th className="p-4 text-sm font-semibold text-gray-700">Appointment Date</th>
                  <th className="p-4 text-sm font-semibold text-gray-700">Status</th>
                  <th className="p-4 text-sm font-semibold text-gray-700">Actions</th>
                </tr>
              </thead>
              <tbody>
                {appointments.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="p-10 text-center text-gray-500 text-sm">No appointments found.</td>
                  </tr>
                ) : (
                  appointments.map((app) => (
                    <tr key={app.id} className="border-b border-gray-100 hover:bg-blue-50/60 transition-colors">
                      <td className="p-4 text-gray-800 font-medium">👤 {getPatientName(app.patientId)}</td>
                      <td className="p-4 text-gray-600">🩺 {getDoctorName(app.doctorId)}</td>
                      <td className="p-4 text-gray-600 font-mono text-sm">{new Date(app.appointmentDate).toLocaleString()}</td>
                      <td className="p-4">
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold shadow-sm ${statusColors[app.status] || 'bg-gray-100'}`}>
                          {app.status}
                        </span>
                      </td>
                      <td className="p-4 flex gap-3 items-center">
                        <select 
                          className="bg-white border border-gray-300 text-gray-700 text-xs rounded-lg focus:ring-blue-500 focus:border-blue-500 block px-2 py-1.5 outline-none cursor-pointer"
                          value={app.status}
                          onChange={(e) => handleStatusUpdate(app.id, e.target.value)}
                        >
                          <option value="Pending">Pending</option>
                          <option value="Completed">Completed</option>
                          <option value="Cancelled">Cancelled</option>
                        </select>
                        <button 
                          onClick={() => handleDelete(app.id)}
                          className="text-red-500 hover:text-white hover:bg-red-500 px-3 py-1.5 rounded-lg transition-colors text-xs font-medium border border-red-200"
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

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Create New Appointment">
        <form onSubmit={handleCreate} className="space-y-5 mt-2">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Select Patient</label>
            <select 
              className="w-full px-4 py-2 bg-white border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-400 focus:border-blue-400 outline-none transition-all cursor-pointer"
              value={formData.patientId}
              onChange={(e) => setFormData({...formData, patientId: e.target.value})}
              required
            >
              <option value="">-- Choose Patient --</option>
              {patients.map(p => (
                <option key={p.id} value={p.id}>{p.fullName || `${p.firstName} ${p.lastName}`}</option>
              ))}
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Select Doctor</label>
            <select 
              className="w-full px-4 py-2 bg-white border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-400 focus:border-blue-400 outline-none transition-all cursor-pointer"
              value={formData.doctorId}
              onChange={(e) => setFormData({...formData, doctorId: e.target.value})}
              required
            >
              <option value="">-- Choose Doctor --</option>
              {doctors.map(d => (
                <option key={d.id} value={d.id}>Dr. {d.fullName || `${d.firstName} ${d.lastName}`}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Appointment Date & Time</label>
            <input 
              type="datetime-local" 
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-400 focus:border-blue-400 outline-none transition-all"
              value={formData.appointmentDate}
              onChange={(e) => setFormData({...formData, appointmentDate: e.target.value})}
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select 
              className="w-full px-4 py-2 bg-white border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-400 focus:border-blue-400 outline-none transition-all"
              value={formData.status}
              onChange={(e) => setFormData({...formData, status: e.target.value})}
            >
              <option value="Pending">Pending</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>

          <div className="pt-5 border-t border-gray-100 flex justify-end gap-3">
            <Button type="button" variant="secondary" onClick={() => setIsModalOpen(false)}>Cancel</Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save Appointment'}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
