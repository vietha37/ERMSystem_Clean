using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<List<Prescription>> GetAllAsync();
        Task<Prescription?> GetByIdAsync(Guid id);
        Task<Prescription?> GetByMedicalRecordIdAsync(Guid medicalRecordId);
        Task AddAsync(Prescription prescription);
        void Delete(Prescription prescription);
        Task<bool> MedicalRecordExistsAsync(Guid medicalRecordId);
        Task<bool> PrescriptionExistsForMedicalRecordAsync(Guid medicalRecordId);
    }
}
