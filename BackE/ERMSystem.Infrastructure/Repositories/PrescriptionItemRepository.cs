using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;
using ERMSystem.Infrastructure.Data;

namespace ERMSystem.Infrastructure.Repositories
{
    public class PrescriptionItemRepository : IPrescriptionItemRepository
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PrescriptionItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.PrescriptionItems.FindAsync(new object[] { id }, ct);

        public async Task AddAsync(PrescriptionItem item, CancellationToken ct = default)
        {
            await _context.PrescriptionItems.AddAsync(item, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(PrescriptionItem item, CancellationToken ct = default)
        {
            _context.PrescriptionItems.Remove(item);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> MedicineExistsAsync(Guid medicineId, CancellationToken ct = default)
            => await _context.Medicines.AnyAsync(m => m.Id == medicineId, ct);

        public async Task<bool> PrescriptionExistsAsync(Guid prescriptionId, CancellationToken ct = default)
            => await _context.Prescriptions.AnyAsync(p => p.Id == prescriptionId, ct);
    }
}
