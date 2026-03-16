using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;
using ERMSystem.Infrastructure.Data;

namespace ERMSystem.Infrastructure.Repositories
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MedicalRecord>> GetAllAsync()
        {
            return await _context.MedicalRecords
                .Include(r => r.Appointment)
                .ToListAsync();
        }

        public async Task<MedicalRecord?> GetByIdAsync(Guid id)
        {
            return await _context.MedicalRecords
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _context.MedicalRecords
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }

        public async Task AddAsync(MedicalRecord record)
        {
            await _context.MedicalRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public void Update(MedicalRecord record)
        {
            _context.MedicalRecords.Update(record);
            _context.SaveChanges();
        }

        public void Delete(MedicalRecord record)
        {
            _context.MedicalRecords.Remove(record);
            _context.SaveChanges();
        }

        public async Task<bool> AppointmentExistsAsync(Guid appointmentId)
        {
            return await _context.Appointments.AnyAsync(a => a.Id == appointmentId);
        }

        public async Task<bool> MedicalRecordExistsForAppointmentAsync(Guid appointmentId)
        {
            return await _context.MedicalRecords.AnyAsync(r => r.AppointmentId == appointmentId);
        }
    }
}
