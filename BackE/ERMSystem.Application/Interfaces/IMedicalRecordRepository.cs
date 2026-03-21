using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task<List<MedicalRecord>> GetAllAsync(CancellationToken ct = default);
        Task<(IEnumerable<MedicalRecord> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Dictionary<string, int>> GetTopDiagnosesAsync(int count, CancellationToken ct = default);
        Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default);
        Task AddAsync(MedicalRecord record, CancellationToken ct = default);
        Task UpdateAsync(MedicalRecord record, CancellationToken ct = default);
        Task DeleteAsync(MedicalRecord record, CancellationToken ct = default);
        Task<bool> AppointmentExistsAsync(Guid appointmentId, CancellationToken ct = default);
        Task<bool> MedicalRecordExistsForAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    }
}
