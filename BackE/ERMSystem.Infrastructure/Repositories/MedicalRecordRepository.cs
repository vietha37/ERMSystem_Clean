using System;
using System.Collections.Generic;
using System.Threading;
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

        public async Task<List<MedicalRecord>> GetAllAsync(CancellationToken ct = default)
            => await _context.MedicalRecords
                .Include(r => r.Appointment)
                .ToListAsync(ct);

        public async Task<(IEnumerable<MedicalRecord> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var totalCount = await _context.MedicalRecords.CountAsync(ct);
            var items = await _context.MedicalRecords
                .Include(r => r.Appointment)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            return (items, totalCount);
        }

        public async Task<Dictionary<string, int>> GetTopDiagnosesAsync(int count, CancellationToken ct = default)
        {
            return await _context.MedicalRecords
                .Where(m => !string.IsNullOrEmpty(m.Diagnosis))
                .GroupBy(m => m.Diagnosis)
                .Select(g => new { Diagnosis = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToDictionaryAsync(x => x.Diagnosis, x => x.Count, ct);
        }

        public async Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.MedicalRecords
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default)
            => await _context.MedicalRecords
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId, ct);

        public async Task AddAsync(MedicalRecord record, CancellationToken ct = default)
        {
            await _context.MedicalRecords.AddAsync(record, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(MedicalRecord record, CancellationToken ct = default)
        {
            _context.MedicalRecords.Update(record);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(MedicalRecord record, CancellationToken ct = default)
        {
            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> AppointmentExistsAsync(Guid appointmentId, CancellationToken ct = default)
            => await _context.Appointments.AnyAsync(a => a.Id == appointmentId, ct);

        public async Task<bool> MedicalRecordExistsForAppointmentAsync(Guid appointmentId, CancellationToken ct = default)
            => await _context.MedicalRecords.AnyAsync(r => r.AppointmentId == appointmentId, ct);
    }
}
