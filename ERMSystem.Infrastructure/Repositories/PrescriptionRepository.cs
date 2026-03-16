using System;
using System.Collections.Generic;
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

        public async Task<List<Prescription>> GetAllAsync()
        {
            return await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .ToListAsync();
        }

        public async Task<Prescription?> GetByIdAsync(Guid id)
        {
            return await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Prescription?> GetByMedicalRecordIdAsync(Guid medicalRecordId)
        {
            return await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.MedicalRecordId == medicalRecordId);
        }

        public async Task AddAsync(Prescription prescription)
        {
            await _context.Prescriptions.AddAsync(prescription);
            await _context.SaveChangesAsync();
        }

        public void Delete(Prescription prescription)
        {
            _context.Prescriptions.Remove(prescription);
            _context.SaveChanges();
        }

        public async Task<bool> MedicalRecordExistsAsync(Guid medicalRecordId)
        {
            return await _context.MedicalRecords.AnyAsync(m => m.Id == medicalRecordId);
        }

        public async Task<bool> PrescriptionExistsForMedicalRecordAsync(Guid medicalRecordId)
        {
            return await _context.Prescriptions.AnyAsync(p => p.MedicalRecordId == medicalRecordId);
        }
    }
}
