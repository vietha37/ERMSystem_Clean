using System;
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

        public async Task<PrescriptionItem?> GetByIdAsync(Guid id)
        {
            return await _context.PrescriptionItems.FindAsync(id);
        }

        public async Task AddAsync(PrescriptionItem item)
        {
            await _context.PrescriptionItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public void Delete(PrescriptionItem item)
        {
            _context.PrescriptionItems.Remove(item);
            _context.SaveChanges();
        }

        public async Task<bool> MedicineExistsAsync(Guid medicineId)
        {
            return await _context.Medicines.AnyAsync(m => m.Id == medicineId);
        }

        public async Task<bool> PrescriptionExistsAsync(Guid prescriptionId)
        {
            return await _context.Prescriptions.AnyAsync(p => p.Id == prescriptionId);
        }
    }
}
