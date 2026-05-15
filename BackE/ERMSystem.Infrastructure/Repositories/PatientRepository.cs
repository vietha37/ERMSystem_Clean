using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;
using ERMSystem.Infrastructure.Data;

namespace ERMSystem.Infrastructure.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Patient>> GetAllAsync(CancellationToken ct = default)
            => await _context.Patients.ToListAsync(ct);

        public async Task<(IEnumerable<Patient> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? textSearch = null,
            CancellationToken ct = default)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(textSearch))
            {
                var keyword = textSearch.Trim();
                var pattern = $"%{keyword}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.FullName, pattern) ||
                    EF.Functions.Like(p.Phone, pattern) ||
                    EF.Functions.Like(p.Address, pattern) ||
                    EF.Functions.Like(p.Gender, pattern) ||
                    (p.EmergencyContactName != null && EF.Functions.Like(p.EmergencyContactName, pattern)) ||
                    (p.EmergencyContactPhone != null && EF.Functions.Like(p.EmergencyContactPhone, pattern)) ||
                    (p.EmergencyContactRelationship != null && EF.Functions.Like(p.EmergencyContactRelationship, pattern)));
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            return (items, totalCount);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
            => await _context.Patients.CountAsync(ct);

        public async Task<Dictionary<DateTime, int>> GetCreatedCountByDayAsync(
            DateTime fromUtc,
            CancellationToken ct = default)
        {
            return await _context.Patients
                .AsNoTracking()
                .Where(p => p.CreatedAt >= fromUtc)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count, ct);
        }

        public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Patients.FindAsync(new object[] { id }, ct);

        public async Task<Patient?> GetByAppUserIdAsync(Guid appUserId, CancellationToken ct = default)
            => await _context.Patients.FirstOrDefaultAsync(p => p.AppUserId == appUserId, ct);

        public async Task<int> GetAppointmentCountAsync(Guid patientId, CancellationToken ct = default)
            => await _context.Appointments.CountAsync(a => a.PatientId == patientId, ct);

        public async Task<IReadOnlyCollection<Patient>> GetPotentialDuplicateCandidatesAsync(
            Guid patientId,
            string fullName,
            DateTime dateOfBirth,
            string phone,
            string? emergencyContactPhone,
            CancellationToken ct = default)
        {
            var query = _context.Patients
                .AsNoTracking()
                .Where(p => p.Id != patientId);

            query = query.Where(p =>
                p.Phone == phone ||
                (p.FullName == fullName && p.DateOfBirth == dateOfBirth) ||
                (!string.IsNullOrWhiteSpace(emergencyContactPhone) && p.EmergencyContactPhone == emergencyContactPhone));

            return await query
                .OrderBy(p => p.FullName)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<int> ReassignAppointmentsAsync(Guid sourcePatientId, Guid targetPatientId, CancellationToken ct = default)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == sourcePatientId)
                .ToListAsync(ct);

            foreach (var appointment in appointments)
            {
                appointment.PatientId = targetPatientId;
            }

            return appointments.Count;
        }

        public async Task AddAsync(Patient patient, CancellationToken ct = default)
        {
            await _context.Patients.AddAsync(patient, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Patient patient, CancellationToken ct = default)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync(ct);
        }

        public async Task MergeAsync(Patient sourcePatient, Patient targetPatient, CancellationToken ct = default)
        {
            _context.Patients.Update(targetPatient);
            _context.Patients.Remove(sourcePatient);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Patient patient, CancellationToken ct = default)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync(ct);
        }
    }
}
