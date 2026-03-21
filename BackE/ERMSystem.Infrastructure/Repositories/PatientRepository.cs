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
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Patient>> GetAllAsync(CancellationToken ct = default)
            => await _context.Patients.ToListAsync(ct);

        public async Task<(IEnumerable<Patient> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var totalCount = await _context.Patients.CountAsync(ct);
            var items = await _context.Patients
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            return (items, totalCount);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
            => await _context.Patients.CountAsync(ct);

        public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Patients.FindAsync(new object[] { id }, ct);

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

        public async Task DeleteAsync(Patient patient, CancellationToken ct = default)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync(ct);
        }
    }
}
