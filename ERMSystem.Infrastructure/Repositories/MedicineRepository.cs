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
    public class MedicineRepository : IMedicineRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicineRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Medicine>> GetAllAsync(CancellationToken ct = default)
            => await _context.Medicines.ToListAsync(ct);

        public async Task<(IEnumerable<Medicine> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var totalCount = await _context.Medicines.CountAsync(ct);
            var items = await _context.Medicines
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            return (items, totalCount);
        }

        public async Task<Medicine?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Medicines.FindAsync(new object[] { id }, ct);

        public async Task AddAsync(Medicine medicine, CancellationToken ct = default)
        {
            await _context.Medicines.AddAsync(medicine, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Medicine medicine, CancellationToken ct = default)
        {
            _context.Medicines.Update(medicine);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Medicine medicine, CancellationToken ct = default)
        {
            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync(ct);
        }
    }
}
