using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<List<Prescription>> GetAllAsync(CancellationToken ct = default);
        Task<(IEnumerable<Prescription> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Dictionary<DateTime, int>> GetCreatedCountByDayAsync(DateTime fromUtc, CancellationToken ct = default);
        Task<Prescription?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Prescription?> GetByMedicalRecordIdAsync(Guid medicalRecordId, CancellationToken ct = default);
        Task AddAsync(Prescription prescription, CancellationToken ct = default);
        Task DeleteAsync(Prescription prescription, CancellationToken ct = default);
        Task<bool> MedicalRecordExistsAsync(Guid medicalRecordId, CancellationToken ct = default);
        Task<bool> PrescriptionExistsForMedicalRecordAsync(Guid medicalRecordId, CancellationToken ct = default);
    }
}
