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
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllAsync(CancellationToken ct = default)
            => await _context.Doctors.ToListAsync(ct);

        public async Task<(IEnumerable<Doctor> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var totalCount = await _context.Doctors.CountAsync(ct);
            var items = await _context.Doctors
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            return (items, totalCount);
        }

        public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Doctors.FindAsync(new object[] { id }, ct);

        public async Task AddAsync(Doctor doctor, CancellationToken ct = default)
        {
            await _context.Doctors.AddAsync(doctor, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Doctor doctor, CancellationToken ct = default)
        {
            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Doctor doctor, CancellationToken ct = default)
        {
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync(ct);
        }
    }
}
