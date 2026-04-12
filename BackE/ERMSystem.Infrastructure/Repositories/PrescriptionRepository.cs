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
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Prescription>> GetAllAsync(CancellationToken ct = default)
            => await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .ToListAsync(ct);

        public async Task<(IEnumerable<Prescription> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var totalCount = await _context.Prescriptions.CountAsync(ct);
            var items = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            return (items, totalCount);
        }

        public async Task<Dictionary<DateTime, int>> GetCreatedCountByDayAsync(
            DateTime fromUtc,
            CancellationToken ct = default)
        {
            return await _context.Prescriptions
                .AsNoTracking()
                .Where(p => p.CreatedAt >= fromUtc)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count, ct);
        }

        public async Task<Prescription?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<Prescription?> GetByMedicalRecordIdAsync(Guid medicalRecordId, CancellationToken ct = default)
            => await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.MedicalRecordId == medicalRecordId, ct);

        public async Task AddAsync(Prescription prescription, CancellationToken ct = default)
        {
            await _context.Prescriptions.AddAsync(prescription, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Prescription prescription, CancellationToken ct = default)
        {
            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> MedicalRecordExistsAsync(Guid medicalRecordId, CancellationToken ct = default)
            => await _context.MedicalRecords.AnyAsync(m => m.Id == medicalRecordId, ct);

        public async Task<bool> PrescriptionExistsForMedicalRecordAsync(Guid medicalRecordId, CancellationToken ct = default)
            => await _context.Prescriptions.AnyAsync(p => p.MedicalRecordId == medicalRecordId, ct);
    }
}
